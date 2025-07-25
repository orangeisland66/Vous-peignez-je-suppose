using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent; 
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using backend.Runtime;
using System.Linq;
using backend.Migrations;

namespace backend.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameRoomService _gameRoomService;
        private readonly GameService _gameService;
        private static readonly ConcurrentDictionary<string, int> _connectionPlayerMap = new();
        private readonly OurDbContext _context;

        // 注入TimerService字段 //添加的一个字段 尝试将TimerService放到GameService中
        //private readonly TimerService _timerService;

        private readonly GameRoomRuntimeManager _runtimeManager;

        // 修改的一个函数
        //往里面加入了TimeServcie的参数
        public GameHub(GameRoomService gameRoomService, GameService gameService, OurDbContext context,
            GameRoomRuntimeManager runtimeManager)
        {
            _gameRoomService = gameRoomService;
            _gameService = gameService;
            _context = context;
            _runtimeManager = runtimeManager;
            // _connectionPlayerMap = new ConcurrentDictionary<string, int>();
            //_timerService = timerService;
        }

        // 添加的一个函数 好像问题不在这里
        // 启动回合计时器
        [HubMethodName("StartRoundWithTimer")] //这个是什么意思？？？？？？
        public async Task StartRoundWithTimer(string roomId)
        {
            try
            {

                Console.WriteLine($"收到启动计时器请求，房间ID:{roomId}");

                //验证房间是否存在 
                if (string.IsNullOrEmpty(roomId))
                {
                    Console.WriteLine("我现在在GameHub的StartRoundWithTimer的if语句中，出现了问题");
                    return;
                }

                //调用TimerService启动计时器 这里我不知道 现在在测试这个位置
                await _gameService.StartRoundTimer(roomId, GamePhase.DrawingAndGuessing, 195); //假设每轮60秒

                Console.WriteLine($"计时器已经启动，房间ID：{roomId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("我现在在GameHub的StartRoundWithTimer方法中，出现了问题");
                //Console.WriteLine($"启动计时器失败:{ex.Message}");
                await Clients.Caller.SendAsync("Error", $"启动计时器失败:{ex.Message}");
            }
        }

        // 玩家加入房间（SignalR专用，前端每个玩家建立SignalR连接后必须调用此方法）
        // 只有调用此方法后，SignalR连接才会加入组，房间内广播才有效
        public async Task JoinRoom(string roomId, int userId)
        {
            // Console.WriteLine($"1");

            // 校验房间是否存在
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            // Console.WriteLine($"2");
            if (room == null)
            {
                // Console.WriteLine($"3");
                await Clients.Caller.SendAsync("RoomNotFound", "房间不存在");
                return;
            }
            // Console.WriteLine($"4");
            //      // 获取玩家信息
            //     var player = await _gameService.GetPlayerByIdAsync(userId);
            // if (player == null)
            // {
            //     // 如果玩家不存在，创建新玩家对象
            //     Console.WriteLine($"创建新玩家: UserId={userId}");
            //     player = new Player
            //     {
            //         UserId = userId, // 关联用户ID
            //         Username = $"玩家{userId}", // 默认用户名，可从用户服务获取
            //         Score = 0,
            //         Status = PlayerStatus.Waiting,
            //         // 设置其他必要属性
            //     };

            //     // 保存新玩家到数据库
            //     await _gameService.CreatePlayerAsync(player);
            //     Console.WriteLine($"新玩家已创建: Id={player.Id}, Username={player.Username}");
            // }
            // SignalR连接加入组
            var player = await _gameService.GetPlayerByUserIdAsync(userId);
            
            Console.WriteLine($"当前用户信息：{player.Id},{player.Username}");
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            _connectionPlayerMap[Context.ConnectionId] = userId;
            Console.WriteLine($"已将连接 ID {Context.ConnectionId} 映射到玩家 ID {userId}");
            // 通知房间内其他玩家
            await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", player.Username);
            await Clients.Caller.SendAsync("JoinedRoom", roomId);
            // 校验玩家是否已经在房间内
            var existingPlayer = room.Players.FirstOrDefault(p => p.UserId == userId);
            if (existingPlayer != null)
            {
                Console.WriteLine($"玩家已在房间中");
                await Clients.Caller.SendAsync("PlayerAlreadyInRoom", "玩家已在房间中");
                return;
            }
            room.Players.Add(player);
            // 保存数据库更改
            await _gameRoomService.UpdateRoomAsync(room);


            Console.WriteLine($"[SignalR] 已将连接 ID {Context.ConnectionId} 映射到玩家 ID {userId}");

            // 立即打印当前 _connectionPlayerMap 内容，便于调试
            Console.WriteLine("[SignalR] 当前所有连接映射：");
            foreach (var kvp in _connectionPlayerMap)
            {
                Console.WriteLine($"连接ID: {kvp.Key}, 玩家ID: {kvp.Value}");
            }

        }
        public async Task LeaveRoom(string roomId, int userId)
        {
            Console.WriteLine($"[SignalR] 玩家 {userId} 请求离开房间 {roomId}");
            // 校验房间是否存在
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("RoomNotFound", "房间不存在");
                return;
            }

            // 获取玩家信息
            var player = await _gameService.GetPlayerByUserIdAsync(userId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }

            // 校验玩家是否在房间内
            var existingPlayer = room.Players.FirstOrDefault(p => p.UserId == userId);
            if (existingPlayer == null)
            {
                await Clients.Caller.SendAsync("PlayerNotInRoom", "玩家不在此房间中");
                return;
            }

            // 从SignalR组中移除连接
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            
            // 移除连接映射
            if (_connectionPlayerMap.ContainsKey(Context.ConnectionId))
            {
                _connectionPlayerMap.Remove(Context.ConnectionId, out _);
                Console.WriteLine($"[SignalR] 已移除连接 ID {Context.ConnectionId} 的玩家映射");
            }

            // 从房间中移除玩家（根据业务需求决定是否彻底移除或标记状态）
            room.Players.Remove(existingPlayer);
            await _gameRoomService.UpdateRoomAsync(room);

            // 通知房间内其他玩家
            await Clients.Group(roomId.ToString()).SendAsync("PlayerLeft", player.Username);
            await Clients.Caller.SendAsync("LeftRoom", roomId);

            // 调试信息：打印当前剩余连接映射
            Console.WriteLine("[SignalR] 玩家离开后剩余连接映射：");
            foreach (var kvp in _connectionPlayerMap)
            {
                Console.WriteLine($"连接ID: {kvp.Key}, 玩家ID: {kvp.Value}");
            }

            // // 额外逻辑：如果房间为空，可考虑删除房间（根据业务需求）
            // if (room.Players.Count == 0)
            // {
            //     await _gameRoomService.DeleteRoomAsync(room.Id);
            //     Console.WriteLine($"[SignalR] 房间 {roomId} 已空，已自动删除");
            // }
        }
        //玩家退出登录
        public async Task Logout(int roomId)
        {
            if (_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                _connectionPlayerMap.Remove(Context.ConnectionId, out _);
                await Clients.Group(roomId.ToString()).SendAsync("UserLoggedOut", playerId);
            }
        }

        // 玩家开始作画
        public async Task PlayerDraw(int roomId, Stroke stroke)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法作画");
                return;
            }

            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            stroke.PlayerId = playerId;
            var result = await _gameService.HandleDrawingAsync(roomId, playerId, stroke);
            if (result)
            {
                await Clients.Group(roomId.ToString()).SendAsync("PlayerDrawing", stroke);
            }
            else
            {
                await Clients.Caller.SendAsync("DrawingFailed", "作画失败");
            }
        }

        // 玩家进行猜词
        public async Task PlayerGuess(int roomId, string guessedWord)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法猜词");
                return;
            }

            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            var result = await _gameService.HandleGuessAsync(roomId, playerId, guessedWord);
            if (result.Success)
            {
                if (result.IsCorrect)
                {
                    await Clients.Group(roomId.ToString()).SendAsync("CorrectGuess", guessedWord);
                }
                else
                {
                    await Clients.Group(roomId.ToString()).SendAsync("IncorrectGuess", guessedWord);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("GuessFailed", result.Message);
            }
        }

        // 开始游戏
        public async Task StartRoomGame(string roomId, int userId)
        {
            // 1. 验证输入参数
            if (string.IsNullOrWhiteSpace(roomId))
            {
                await Clients.Caller.SendAsync("GameStartFailed", "房间ID (roomId) 不能为空。");
                return;
            }
            
            if (userId <= 0)
            {
                await Clients.Caller.SendAsync("GameStartFailed", "用户ID (userId) 无效或缺失。");
                return;
            }

            // 2. 调用服务层方法
            var result = await _gameRoomService.StartGameByRoomIdStringAsync(roomId, userId);

                // 3. 根据服务层返回的结果推送消息
                if (result.Success)
                {
                    // 获取房间所有参与者的连接ID
                    //var connectionIds = await GetRoomConnectionIds(roomIdString);

                    // 向房间内所有客户端推送游戏开始消息
                    await Clients.Group(roomId.ToString()).SendAsync("GameStarted", new
                    {
                        success = true,
                        message = result.Message,
                        roomId = roomId
                    });
                    await NewRound(roomId, userId);
    
                }
                else
                {
                    // 向调用者推送失败消息
                    await Clients.Caller.SendAsync("GameStartFailed", new
                    {
                        success = false,
                        message = result.Message
                    });
                }
        }
        private async Task SendStatus(string roomId, ActiveGameState activeState)
        {
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);

            // 5.1 向房间内所有客户端推送通用游戏状态
            var gameStateForAll = new
            {
                currentRound = activeState.CurrentRound,
                totalRounds = activeState.TotalRounds,
                currentPhase = activeState.CurrentGamePhase,
                currentPainterUserId = activeState.CurrentPainterUserId,
                currentPainterUsername = room.Players.FirstOrDefault(p => p.UserId == activeState.CurrentPainterUserId)?.Username,
                playerScores = activeState.PlayerScoresJson,
                // 不包含词语选项（避免非画师看到）
            };
            await Clients.Group(roomId).SendAsync("GameStateUpdated", gameStateForAll);
            Console.WriteLine($"[GameHub.StartFirstRound] 已向房间 {roomId} 推送游戏状态");
        }
        private async Task UpdateGameStatus(string mode, string roomId, ActiveGameState activeState)
        {
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (mode == "completeSelection")
            {
                await SendStatus(roomId, activeState); // 确保状态更新
            }
            if (mode == "selection")
                {
                    await _gameRoomService.StartNextRoundAsync(activeState, room.Players);
                    if (activeState.CurrentPainterUserId.HasValue)
                    {
                        var painterUserId = activeState.CurrentPainterUserId.Value;
                        // 关键：通过 _connectionPlayerMap 反向查询画师的所有连接 ID
                        var painterConnectionIds = _connectionPlayerMap
                            .Where(kvp => kvp.Value == painterUserId) // 匹配用户 ID
                            .Select(kvp => kvp.Key) // 提取连接 ID
                            .ToList();

                        // 验证画师是否有活跃连接
                        if (!painterConnectionIds.Any())
                        {
                            Console.WriteLine($"[GameHub.UpdateGameStatus] 警告：画师 {painterUserId} 没有活跃连接，无法发送词语选项");
                            // 可选：通知房间内其他玩家或处理错误
                            await Clients.Group(roomId).SendAsync("SystemMessage", $"画师 {painterUserId} 已离线，无法继续游戏");
                            return;
                        }

                        // 构建发送给画师的词语选项
                        var wordChoicesForPainter = new
                        {
                            choices = activeState.WordChoicesForPainter,
                            tip = "请选择一个词进行绘画",
                            painterUserId = painterUserId // 前端可验证是否自己是画师
                        };

                        // 只向画师的连接发送消息（而非整个房间）
                        await Clients.Clients(painterConnectionIds).SendAsync("WordChoicesAvailable", wordChoicesForPainter);
                        Console.WriteLine($"[GameHub.UpdateGameStatus] 已向画师 {painterUserId} 推送词语选项（连接数：{painterConnectionIds.Count}）");

                        // 同时更新房间内所有玩家的游戏状态（不含词语选项）
                        await SendStatus(roomId, activeState);
                    }
                        try
                    {
                        // 启动选词计时器
                        await _gameService.StartRoundTimer(roomId, GamePhase.WaitingForPainterToChooseWord, 15); // 假设选词阶段15秒
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GameHub.StartFirstRound] 启动计时器失败: {ex.Message}");
                        await Clients.Caller.SendAsync("Error", "启动计时器失败");
                    }
                }
                else if (mode == "completeSelection")
                {
                    try
                    {
                        // 启动选词计时器
                        await _gameService.StartRoundTimer(roomId, GamePhase.WaitingForPainterToChooseWord, 180); // 假设选词阶段180秒
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GameHub.StartFirstRound] 启动计时器失败: {ex.Message}");
                        await Clients.Caller.SendAsync("Error", "启动计时器失败");
                    }
                }
                else
                {
                    throw new ArgumentException("无效的模式参数", nameof(mode));
                }



        }
        public async Task ConfirmWordSelection(string roomId, string selectedWord)
        {
            if (!_runtimeManager.TryGetState(roomId, out var activeState))
            {
                Console.WriteLine("当前房间没有运行时状态，请检查游戏是否已初始化");
                throw new Exception("房间状态不存在");
            }

            // 更新当前回合的目标单词
            activeState.CurrentTargetWord = selectedWord;
            activeState.CurrentGamePhase = GamePhase.DrawingAndGuessing;
            //await _gameService.StopTimer(roomId);
            await UpdateGameStatus("completeSelection", roomId, activeState);
            Console.WriteLine($"[GameHub.ConfirmWordSelection] 画师已选择词语: {selectedWord}");

            // Console.WriteLine($"[GameHub.ConfirmWordSelection] 已向房间 {roomId} 推送选词结果: {selectedWord}");
        }
        public async Task NewRound(string roomId, int userId)
        {
            if (!_runtimeManager.TryGetState(roomId, out var activeState))
            {
                Console.WriteLine("当前房间没有运行时状态，请检查游戏是否已初始化");
                throw new Exception("房间状态不存在");
            }
            try
            {
                await UpdateGameStatus("selection", roomId, activeState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameHub.StartFirstRound] 发送SignalR消息失败: {ex.Message}");
                // 通知客户端但不中断流程（状态已保存，仅推送失败）
                await Clients.Caller.SendAsync("Warning", "回合已初始化，但通知可能延迟");
            }
           


        }
        // 切换到下一个轮次
        public async Task NextRound(string roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法进入下一轮");
                return;
            }

           // var result = await _gameService.StartRoundAsync(roomId);
            var result = true; // 假设这里是调用游戏服务的逻辑
            if (result)
            {
                await Clients.Group(roomId.ToString()).SendAsync("NextRound", "进入下一轮！");
            }
            else
            {
                await Clients.Caller.SendAsync("NextRoundFailed", "进入下一轮失败");
            }
        }

        // 游戏结束
        public async Task EndGame(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法结束游戏");
                return;
            }

            var result = await _gameRoomService.EndGameAsync(roomId);
            if (result)
            {
                var gameStatus = await _gameService.GetGameStatusAsync(roomId);
                await Clients.Group(roomId.ToString()).SendAsync("GameEnded", gameStatus);
            }
            else
            {
                await Clients.Caller.SendAsync("EndGameFailed", "游戏结束失败");
            }
        }

        // // 断开连接时
        // public override async Task OnDisconnectedAsync(System.Exception exception)
        // {
        //     string connectionId = Context.ConnectionId;
        //     if (_connectionPlayerMap.ContainsKey(connectionId))
        //     {
        //         int playerId = _connectionPlayerMap[connectionId];
        //         _connectionPlayerMap.Remove(connectionId, out _);
        //         Console.WriteLine($"用户断开连接，已移除映射 - ConnectionId: {connectionId}, PlayerId: {playerId}");
        //     }
        //     await base.OnDisconnectedAsync(exception);
        // }

        // 获取当前连接的房间 ID
        private int? GetRoomIdFromContext()
        {
            if (Context.Items.TryGetValue("RoomId", out var roomId))
            {
                return (int)roomId;
            }
            return null;
        }

        // 实时同步画板数据
        public async Task SyncDrawing(int roomId, List<Point> points)
        {
            // 可根据需要校验房间和玩家身份
            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            // 广播给房间内的所有玩家
            await Clients.OthersInGroup(roomId.ToString()).SendAsync("ReceiveDrawing", new { PlayerId = playerId, Points = points });
        }

        //接收客户端发送的聊天消息，并将消息广播给房间内的所有玩家。
        public async Task SendChatMessage(string roomId, string message)
        {
            try
            {
                // **校验房间是否存在（与JoinRoom逻辑一致）**
                var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
                if (room == null)
                {
                    throw new Exception("房间不存在");
                }

                List<int> scoreList = room.Players.Select(player => player.Score).ToList();
                // **获取当前连接的玩家ID（使用线程安全的ConcurrentDictionary）**
                if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int UserId))
                {
                    Console.WriteLine($"[SignalR] SendChatMessage时未找到玩家信息，当前连接ID: {Context.ConnectionId}");
                    Console.WriteLine("[SignalR] 当前所有连接映射：");
                    foreach (var kvp in _connectionPlayerMap)
                    {
                        Console.WriteLine($"连接ID: {kvp.Key}, 玩家ID: {kvp.Value}");
                    }
                    throw new Exception("未找到玩家信息");
                }

                // 校验玩家是否在房间中（从数据库获取玩家列表，避免依赖内存映射）
                var player = room.Players.FirstOrDefault(p => p.UserId.HasValue && p.UserId.Value == UserId);
                Console.WriteLine($"[SignalR] SendChatMessage - 玩家ID: {UserId}, 玩家用户名: {player?.Username ?? "未知"}");
                if (player == null)
                {
                    throw new Exception("玩家不在当前房间中");
                }
                Console.WriteLine($"1");
                // 构建消息对象
                var chatMessage = new ChatMessage
                {
                    Content = message,
                    SenderId = UserId,
                    GameRoomId = room.RoomId,
                    Timestamp = DateTime.UtcNow,
                    // Sender = player.User // 关联用户信息
                };
                Console.WriteLine($"2");
                // 保存消息到数据库
                try
                {
                    _context.ChatMessages.Add(chatMessage);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存聊天消息失败: {ex.Message}");
                    // 继续执行，即使保存失败也尝试发送消息
                }
                var is_Correct = false;
                if (!_runtimeManager.TryGetState(roomId, out var activeState))
                {
                    Console.WriteLine("当前房间没有运行时状态，请检查游戏是否已初始化");
                    throw new Exception("房间状态不存在");
                }

                //if (message == activeState.CurrentTargetWord)
                Console.WriteLine(room.Status);

                if (message == "苹果"&&!player.HasGuessed) // 假设目标单词是"苹果"，并且玩家还没有猜对
                {
                    is_Correct = true;
                    //更新玩家分数
                    Console.WriteLine($"玩家 {player.Username} 猜对了单词: {message}");
                    player.Score += 1; // 假设猜对加1分
                    player.HasGuessed = true; // 标记玩家已猜对
                                              // 记得每回合更新
                    _context.Players.Update(player);
                }
            try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存聊天消息到数据库失败: {ex.Message}");
                    // 继续执行，即使保存失败也尝试发送消息
                }
                Console.WriteLine($"3");
                // 广播消息给房间内的所有玩家
                try
                {
                    await Clients.Group(roomId).SendAsync("ReceiveChatMessage", new
                    {
                        playerId = UserId,
                        username = player.Username, // 玩家用户名
                        content = message,
                        timestamp = chatMessage.Timestamp.ToString("o"), // ISO 8601 格式时间戳
                        isCorrect = is_Correct, // 是否猜对
                        scores = scoreList // 广播当前分数列表
                    });
                    Console.WriteLine($"广播消息成功，房间ID：{roomId}，玩家ID：{UserId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"广播消息失败: {ex.Message}");
                    await Clients.Caller.SendAsync("ChatError", new { message = "消息发送失败" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送聊天消息失败: {ex.Message}");
                await Clients.Caller.SendAsync("ChatError", new { message = ex.Message });
            }
        }

        // 接受客户端发送的绘图数据，并将数据广播给房间内的所有玩家
        public async Task SendStroke(string roomId, object strokeData)
        {
            Console.WriteLine($"收到绘画数据，房间ID：{roomId}");
            //校验房间是否存在
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (room == null)
            {
                throw new Exception("房间不存在");
            }

            // 从链接映射中获取玩家 ID（确保已加入房间）
            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int UserId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.UserId.HasValue && p.UserId.Value == UserId);
            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(UserId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }
            
            // 构建绘画数据实体
            var strokeInfo = new
            {
                strokeData = strokeData,
                SenderId = UserId,
                GameRoomId = roomId,
                TimeStamp = DateTime.UtcNow
            };

            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", new
            {
                UserId = UserId, //玩家ID
                username = player.Username, //玩家用户名
                strokeData = strokeData, //绘画数据
                timestamp = strokeInfo.TimeStamp.ToString("o")
            });

            Console.WriteLine($"绘画数据已发送，房间ID：{roomId}，玩家ID：{UserId}, 用户名：{player.Username}");
        }

        // 接收客户端发送的撤销操作，并广播到房间内的所有玩家
        public async Task SendUndo(string roomId)
        {
            Console.WriteLine($"收到撤销操作，房间ID：{roomId}");

            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (room == null)
            {
                throw new Exception("房间不存在");
            }

            // 从链接映射中获取玩家 ID（确保已加入房间）
            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int UserId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.UserId.HasValue && p.UserId.Value == UserId);
            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(UserId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }
            
            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveUndo");

            // 调试信息
            Console.WriteLine($"撤销操作已发送，房间ID：{roomId}，玩家ID：{UserId}");
        }

        // 接收客户端发送的重做操作，并广播到房间内的所有玩家
        public async Task SendRedo(string roomId)
        {
            Console.WriteLine($"收到重做操作，房间ID：{roomId}");
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (room == null)
            {
                throw new Exception("房间不存在");
            }

            // 从链接映射中获取玩家 ID（确保已加入房间）
            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int UserId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.UserId.HasValue && p.UserId.Value == UserId);
            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(UserId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }
            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveRedo");

            // 调试信息
            Console.WriteLine($"重做操作已发送，房间ID：{roomId}，玩家ID：{UserId}");
        }

        // 接收客户端发送的清空操作，并广播到房间内的所有玩家
        public async Task SendClear(string roomId)
        {
            Console.WriteLine($"收到清空操作，房间ID：{roomId}");
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
            if (room == null)
            {
                throw new Exception("房间不存在");
            }

            // 从链接映射中获取玩家 ID（确保已加入房间）
            if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int UserId))
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.UserId.HasValue && p.UserId.Value == UserId);
            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(UserId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }
            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveClear");

            // 调试信息
            Console.WriteLine($"清空操作已发送，房间ID：{roomId}，玩家ID：{UserId}");
        }

        // public async Task AddToConnectionMap(int playerId)
        // {
        //     _connectionPlayerMap[Context.ConnectionId] = playerId;
            
        // }

        // 根据房间 ID 获取玩家名称列表
        public async Task<LinkedList<string>> GetPlayerNames(string roomId)
        {
            try
            {
                var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
                if (room == null)
                {
                    await Clients.Caller.SendAsync("RoomNotFound", "房间不存在");
                    return new LinkedList<string>(); // 返回空集合而非null
                }

                Console.WriteLine($"获取房间 {roomId} 的玩家信息：");
                foreach (var player in room.Players)
                {
                    Console.WriteLine($"玩家: {player.Username}, 状态: {player.Status}, 分数: {player.Score}");
                }

                var nameList = room.Players
                    .Select(player => player.Username)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList();

                Console.WriteLine($"玩家名称列表: {string.Join(", ", nameList)}");

                // 转换为LinkedList并返回
                return new LinkedList<string>(nameList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取房间信息失败: {ex.Message}");
                await Clients.Caller.SendAsync("Error", "获取房间信息失败");
                return new LinkedList<string>(); // 异常情况下返回空集合
            }
        }    
        // 打印指定SignalR组（房间）中的玩家信息
        public void PrintGroupPlayers(string roomId)
        {
            Console.WriteLine($"[SignalR] 房间 {roomId} 的玩家连接信息如下：");
            foreach (var kvp in _connectionPlayerMap)
            {
                // 这里只能打印所有连接和玩家ID，无法直接判断是否在该组（SignalR没有直接API获取组成员）
                Console.WriteLine($"连接ID: {kvp.Key}, 玩家ID: {kvp.Value}");
            }
            Console.WriteLine("[SignalR] 请注意：如需精确判断玩家是否在该组，请结合业务层房间数据校验。");
        }
        
        
    }
}
