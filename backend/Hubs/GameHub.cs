using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent; 
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using backend.Data;

namespace backend.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameRoomService _gameRoomService;
        private readonly GameService _gameService;
        private static readonly ConcurrentDictionary<string, int> _connectionPlayerMap = new();
        private readonly OurDbContext _context;

        public GameHub(GameRoomService gameRoomService, GameService gameService, OurDbContext context)
        {
            _gameRoomService = gameRoomService;
            _gameService = gameService;
            _context = context;
            // _connectionPlayerMap = new ConcurrentDictionary<string, int>();
        }

        // 玩家加入房间（SignalR专用，前端每个玩家建立SignalR连接后必须调用此方法）
        // 只有调用此方法后，SignalR连接才会加入组，房间内广播才有效
    public async Task JoinRoom(string roomId, int userId)
    {
        Console.WriteLine($"1");

        // 校验房间是否存在
        var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomId);
        Console.WriteLine($"2");
        if (room == null)
        {
            Console.WriteLine($"3");
            await Clients.Caller.SendAsync("RoomNotFound", "房间不存在");
            return;
        }
        Console.WriteLine($"4");
         // 获取玩家信息
        var player = await _gameService.GetPlayerByIdAsync(userId);
        if (player == null)
        {
            Console.WriteLine($"5");
            await Clients.Caller.SendAsync("PlayerNotFound", "玩家不存在");
            return;
        }

         // SignalR连接加入组
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        _connectionPlayerMap[Context.ConnectionId] = userId;
            // 通知房间内其他玩家
        await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", player.Username);
        await Clients.Caller.SendAsync("JoinedRoom", roomId);
        // 校验玩家是否已经在房间内
            var existingPlayer = room.Players.FirstOrDefault(p => p.Id == userId);
        if (existingPlayer != null)
        {
            Console.WriteLine($"玩家已在房间中");
            await Clients.Caller.SendAsync("PlayerAlreadyInRoom", "玩家已在房间中");
            return;
        }

       
        // 将玩家添加到房间的玩家列表中
        room.Players.Add(player);

        // 保存数据库更改
        await _gameRoomService.UpdateRoomAsync(room);

        Console.WriteLine($"6");

       

        Console.WriteLine($"[SignalR] 已将连接 ID {Context.ConnectionId} 映射到玩家 ID {userId}");

        // 立即打印当前 _connectionPlayerMap 内容，便于调试
        Console.WriteLine("[SignalR] 当前所有连接映射：");
        foreach (var kvp in _connectionPlayerMap)
        {
            Console.WriteLine($"连接ID: {kvp.Key}, 玩家ID: {kvp.Value}");
        }

       
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
        public async Task StartGame(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Waiting)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "房间状态无法开始游戏");
                return;
            }

            var result = await _gameRoomService.StartGameAsync(roomId);
            if (result)
            {
                await Clients.Group(roomId.ToString()).SendAsync("GameStarted", "游戏开始了！");
            }
            else
            {
                await Clients.Caller.SendAsync("StartGameFailed", "游戏开始失败");
            }
        }

        // 切换到下一个轮次
        public async Task NextRound(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法进入下一轮");
                return;
            }

            var result = await _gameService.StartRoundAsync(roomId);
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

        // 断开连接时
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            string connectionId = Context.ConnectionId;
            if (_connectionPlayerMap.ContainsKey(connectionId))
            {
                int playerId = _connectionPlayerMap[connectionId];
                _connectionPlayerMap.Remove(connectionId, out _);
                Console.WriteLine($"用户断开连接，已移除映射 - ConnectionId: {connectionId}, PlayerId: {playerId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // 获取当前连接的房间 ID
        private int? GetRoomIdFromContext()
        {
            if (Context.Items.TryGetValue("RoomId", out var roomId))
            {
                return (int)roomId;
            }
            return null;
        }

        // 下面是我根据后端类图补充的函数
        // OnConnectedAsync()
        public override async Task OnConnectedAsync()
        {
            // 这里可以添加连接时的逻辑，比如记录连接信息等
            await base.OnConnectedAsync();
        }

        // BroadcastStroke()
        public async Task BroadcastStroke(int roomId, Stroke stroke)
        {
            if (_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                stroke.PlayerId = playerId;
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", stroke);
            }
            else
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            }
        }

        // BroadcastGuess
        public async Task BroadcastGuess(int roomId, string guessedWord)
        {
            if (_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveGuess", playerId, guessedWord);
            }
            else
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            }
        }

        // BroadcastChatMessage
        public async Task BroadcastChatMessage(int roomId, string ChatMessage)
        {
            if (_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveChatMessage", playerId, ChatMessage);
            }
            else
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            }
        }

        // HandleNetworkInterrupt
        public async Task HandleNetworkInterrupt(int roomId)
        {
            if (_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                await Clients.Group(roomId.ToString()).SendAsync("NetworkInterrupt", playerId);
            }
            else
            {
                await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            }
        }

        // ReconnectUser
        public async Task ReconnectUser(int roomId)
        {

        }

        // UpdatePlayerStatus
        public async Task UpdatePlayerStatus(int roomId, PlayerStatus status)
        {

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
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存聊天消息失败: {ex.Message}");
            // 继续执行，即使保存失败也尝试发送消息
        }
Console.WriteLine($"3");
        // 广播消息给房间内的所有玩家
        await Clients.Group(roomId.ToString()).SendAsync("ReceiveChatMessage", new
        {
            GameRoomId = roomId,
            SenderId = UserId,
            playerId = player.Id,
            username = player.User?.Username ?? player.Username,
            content = message,
            timestamp = chatMessage.Timestamp.ToString("o"),
        });

        Console.WriteLine($"消息已广播 - 房间ID: {roomId}, 玩家: {player.Username}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"处理聊天消息时出错: {ex.Message}");
        PrintGroupPlayers(roomId);
        await Clients.Caller.SendAsync("ChatError", new { message = ex.Message });
        throw;
    }
}

        // 接受客户端发送的绘图数据，并将数据广播给房间内的所有玩家
        public async Task SendStroke(int roomId, object strokeData)
        {
            Console.WriteLine($"收到绘画数据，房间ID：{roomId}");

            // 从链接映射中获取玩家 ID（确保已加入房间）
            // if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            // {
            //     await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            //     return;
            // }
            // 测试用：硬编码玩家 ID（玩家一的 ID 假设为 1）
            int playerId = 1; // 确保数据库中有ID为1的玩家

            // 假设从数据库获取玩家用户名（测试时可固定为已知用户名）
            var player = new Player { Id = playerId, Username = "玩家一" };

            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }

            // 构建绘画数据实体
            var strokeInfo = new
            {
                strokeData = strokeData,
                SenderId = playerId,
                GameRoomId = roomId,
                TimeStamp = DateTime.UtcNow
            };

            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.All.SendAsync("ReceiveStroke", new
            {
                playerId,
                username = player.Username, //玩家用户名
                strokeData = strokeData, //绘画数据
                timestamp = strokeInfo.TimeStamp.ToString("o")
            });

            Console.WriteLine($"绘画数据已发送，房间ID：{roomId}，玩家ID：{playerId}");
        }

        // 接收客户端发送的撤销操作，并广播到房间内的所有玩家
        public async Task SendUndo(int roomId)
        {
            Console.WriteLine($"收到撤销操作，房间ID：{roomId}");

            // 从链接映射中获取玩家 ID（确保已加入房间）
            // if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            // {
            //     await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            //     return;
            // }
            // 测试用：硬编码玩家 ID（玩家一的 ID 假设为 1）
            int playerId = 1; // 确保数据库中有ID为1的玩家

            // 假设从数据库获取玩家用户名（测试时可固定为已知用户名）
            var player = new Player { Id = playerId, Username = "玩家一" };

            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }

            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.All.SendAsync("ReceiveUndo");

            // 调试信息
            Console.WriteLine($"撤销操作已发送，房间ID：{roomId}，玩家ID：{playerId}");
        }

        // 接收客户端发送的重做操作，并广播到房间内的所有玩家
        public async Task SendRedo(int roomId)
        {
            Console.WriteLine($"收到重做操作，房间ID：{roomId}");

            // 从链接映射中获取玩家 ID（确保已加入房间）
            // if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            // {
            //     await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            //     return;
            // }
            // 测试用：硬编码玩家 ID（玩家一的 ID 假设为 1）
            int playerId = 1; // 确保数据库中有ID为1的玩家

            // 假设从数据库获取玩家用户名（测试时可固定为已知用户名）
            var player = new Player { Id = playerId, Username = "玩家一" };

            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }

            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.All.SendAsync("ReceiveRedo");

            // 调试信息
            Console.WriteLine($"重做操作已发送，房间ID：{roomId}，玩家ID：{playerId}");
        }
        
        // 接收客户端发送的清空操作，并广播到房间内的所有玩家
        public async Task SendClear(int roomId)
        {
            Console.WriteLine($"收到清空操作，房间ID：{roomId}");

            // 从链接映射中获取玩家 ID（确保已加入房间）
            // if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            // {
            //     await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            //     return;
            // }
            // 测试用：硬编码玩家 ID（玩家一的 ID 假设为 1）
            int playerId = 1; // 确保数据库中有ID为1的玩家

            // 假设从数据库获取玩家用户名（测试时可固定为已知用户名）
            var player = new Player { Id = playerId, Username = "玩家一" };

            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }

            // 保存到数据库
            // await _gameRoomService.SaveStrokeDataAsync(roomId, strokeInfo);

            // 广播绘画数据给房间内所有玩家
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveStroke", strokeInfo);
            await Clients.All.SendAsync("ReceiveClear");

            // 调试信息
            Console.WriteLine($"清空操作已发送，房间ID：{roomId}，玩家ID：{playerId}");
        }

        public async Task AddToConnectionMap(string connectionId, int playerId)
        {
            _connectionPlayerMap[connectionId] = playerId;
            Console.WriteLine($"已将连接 ID {connectionId} 映射到玩家 ID {playerId}");
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

