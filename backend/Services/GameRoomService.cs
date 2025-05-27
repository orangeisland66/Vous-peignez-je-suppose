using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using backend.Services;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using backend.Hubs;             

namespace backend.Services
{
    public class GameRoomService
    {
        private readonly OurDbContext _context;
        private readonly IHubContext<GameHub> _hubContext; // <--- 新增字段
        private readonly GameService _gameService; // 如果需要调用其他游戏逻辑服务
        public GameRoomService(OurDbContext context, IHubContext<GameHub> hubContext, GameService gameService)
        {
            _context = context;
            _hubContext = hubContext;
            _gameService = gameService; // 如果需要调用其他游戏逻辑服务
        }

        /// <summary>
        /// 创建游戏房间
        /// </summary>
        /// <param name="gameRoom">要创建的游戏房间对象</param>
        /// <returns>创建成功的游戏房间对象</returns>
        public async Task<GameRoom> CreateRoomAsync(GameRoom roomDataFromController)
        {
            // **1. 查找创建者用户**
            // 假设你的 User 模型可以通过 Id 查找，并且 Player 模型需要 User 信息
            var creator = await _context.Users.FindAsync(roomDataFromController.CreatorId);

            if (creator == null)
            {
                // 选择抛出异常，让 Controller 捕获并返回 BadRequest
                throw new ArgumentException("创建者用户不存在。");
            }

            // **2. 创建并填充 GameRoom 实体**
            // roomDataFromController 已经包含了大部分前端设置的属性
            // GameRoom 的构造函数会初始化 Players 和 ChatHistory 为空列表
            // 以及设置一些默认值，这些默认值会被 roomDataFromController 中的值覆盖（如果存在）

            // 确保 RoomId 被正确设置 (前端已生成并发送)
            // newRoom.RoomId = roomDataFromController.RoomId; (如果模型绑定没问题，这里应该是对的)

            // 关联创建者 (如果你的 GameRoom 模型有 Creator 导航属性)
            roomDataFromController.Creator = creator; // 如果 CreatorId 已正确绑定，这可能不需要显式设置，取决于你的EF配置

            // **3. 将创建者添加为第一个玩家 (房主)**
            // 假设你的 Player 模型如下：
            // public class Player { public int Id {get;set;} public int UserId {get;set;} public virtual User User {get;set;} public string Username {get;set;} public bool IsHost {get;set;} /* ...其他属性... */ }
            var hostPlayer = new Player
            {
                // Player.Id 是数据库自动生成的，这里不应该用 creator.Id 赋值给 Player.Id
                // Player.Id 将在 SaveChangesAsync() 后由数据库生成并填充

                UserId = creator.Id,          // **设置关联的 User 的 ID**
                User = creator,               // **设置导航属性到 User 对象**
                Username = creator.Username,  // **从 User 对象获取用户名作为 Player 的初始用户名**
                IsHost = true,                // 标记为房主
                GameRoom = roomDataFromController, // **关联到当前创建的 GameRoom 对象**
                                                   // GameRoomId 会在 EF Core 保存时根据 GameRoom 导航属性自动设置
                                                   // 其他 Player 属性会使用 Player 构造函数中的默认值 (Score=0, Status=Waiting, etc.)
            };
            // GameRoom 的构造函数应该已经初始化了 Players = new List<Player>();
            roomDataFromController.Players.Add(hostPlayer);

            // **4. 保存到数据库**
            _context.GameRooms.Add(roomDataFromController);
            // 当你保存 GameRoom 时，如果 Player 与 GameRoom 有正确的导航属性和外键关系，
            // EF Core 会自动将 hostPlayer 也保存到 Players 表并建立关联。
            await _context.SaveChangesAsync();

            return roomDataFromController; // 返回创建并保存到数据库的房间对象 (现在它有DB生成的Id)
        }

        /// <summary>
        /// 获取所有游戏房间列表
        /// </summary>
        /// <returns>包含所有游戏房间的列表</returns>
        public async Task<List<GameRoom>> GetAllRoomsAsync()
        {
            return await _context.GameRooms
                .Include(gr => gr.Players)
                .Include(gr => gr.ChatHistory)
                .ToListAsync();
        }

        /// <summary>
        /// 根据房间 ID 获取游戏房间
        /// </summary>
        /// <param name="roomId">游戏房间的 ID</param>
        /// <returns>对应的游戏房间对象，如果不存在则返回 null</returns>
        public async Task<GameRoom?> GetRoomDetailsAsync(int roomId)
        {
            return await _context.GameRooms
                .Include(gr => gr.Players)
                .Include(gr => gr.Creator)      // 加载房间的创建者 (User)
                .Include(gr => gr.Players)      // 加载房间内的所有玩家
                    .ThenInclude(p => p.User)   // 对于每个玩家，加载其关联的 User 信息
                                                // .Include(gr => gr.ChatHistory) // 如果等待界面不需要聊天记录，可以暂时不加载以提高性能
                .FirstOrDefaultAsync(gr => gr.Id == roomId);
        }

        /// <summary>
        /// 根据自定义的字符串 RoomId (例如8位房间号) 获取房间详细信息。
        /// 会加载 Creator, Players (及其关联的 User)。
        /// </summary>
        /// <param name="roomIdString">房间的自定义字符串 ID。</param>
        /// <returns>找到的 GameRoom 对象，如果不存在则返回 null。</returns>
        public async Task<GameRoom?> GetRoomDetailsByRoomIdStringAsync(string roomIdString)
        {

            return await _context.GameRooms
                .Include(gr => gr.Creator)      // 加载房间的创建者 (User)
                .Include(gr => gr.Players)      // 加载房间内的所有玩家
                    .ThenInclude(p => p.User)   // 对于每个玩家，加载其关联的 User 信息
                .Include(gr => gr.ActiveState) // <--- 新增这一行来加载活动游戏状态
                                                // .Include(gr => gr.ChatHistory) // 根据需要加载
                .FirstOrDefaultAsync(gr => gr.RoomId == roomIdString); // 条件是自定义的 RoomId 字符串
        }


        /// <summary>
        /// 用户加入游戏房间
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="player">要加入房间的玩家</param>
        /// <returns>加入成功返回 true，否则返回 false</returns>
        public async Task<bool> JoinRoomAsync(string roomId, string userId, Player player)
        {

            var gameRoom = await GetRoomDetailsByRoomIdStringAsync(roomId);

            if (gameRoom == null)
            {

                return false;
            }

            if (gameRoom.IsPrivate)
            {
                //    Console.WriteLine($"房间不存在");
                return false;
            }
            //确保不是重复加入
            if (gameRoom.Players.Any(p => p.UserId.ToString() == userId))
            {
                return false;
            }

            // 加载用户
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user == null)
            {
                return false; // 用户不存在
            }

            // 设置 Player 的用户属性和房间属性
            player.User = user;
            player.UserId = user.Id;
            player.GameRoomId = gameRoom.RoomId;
            player.JoinedAt = DateTime.UtcNow;

            // 添加到房间的玩家列表
            gameRoom.Players.Add(player);

            await _context.SaveChangesAsync();
            Console.WriteLine($"用户 {user.Username} 加入房间 {roomId}");
            return true;
        }

        /// <summary>
        /// 处理玩家离开游戏房间的请求。
        /// 如果是房主离开，则解散房间。
        /// 如果是普通玩家离开，则将其从房间移除。
        /// </summary>
        /// <param name="roomIdString">房间的字符串ID (例如8位房间号)。</param>
        /// <param name="requestingUserId">发起请求的用户的ID。</param>
        /// <returns>一个包含操作结果的对象，指明操作是否成功以及房间是否被解散。</returns>
        public async Task<LeaveRoomResult> HandlePlayerLeavingAsync(string roomIdString, int requestingUserId)
        {
            // 使用 GetRoomDetailsByRoomIdStringAsync 来加载房间，因为它会加载必要的关联数据
            var room = await _context.GameRooms
                .Include(gr => gr.Creator)      // 确保 Creator 被加载
                .Include(gr => gr.Players)      // 加载房间内的所有玩家
                    .ThenInclude(p => p.User)   // 对于每个玩家，加载其关联的 User 信息
                .FirstOrDefaultAsync(gr => gr.RoomId == roomIdString);

            if (room == null)
            {
                return new LeaveRoomResult { Success = false, Message = "房间不存在。" };
            }

            var playerLeavingRecord = room.Players.FirstOrDefault(p => p.UserId == requestingUserId);

            if (playerLeavingRecord == null)
            {
                // 用户可能已经不在房间，或者数据有误。
                // 这种情况可以认为操作“成功”（用户不在房间，等于已离开）或者返回特定错误。
                // 为了简单起见，我们认为如果记录不存在，就不能执行“离开”操作。
                return new LeaveRoomResult { Success = false, Message = "请求的用户不在该房间内。" };
            }

            // 判断是否为房主。优先使用 Player 记录中的 IsHost 标志。
            // 确保在 CreateRoomAsync 中，房主的 Player.IsHost 被正确设置为 true。
            bool isHostLeaving = playerLeavingRecord.IsHost;

            //添加一个备用检查：
            if (!isHostLeaving && room.CreatorId == requestingUserId)
            {
                isHostLeaving = true;
            }

            if (isHostLeaving)
            {
                // 房主离开，解散房间
                // TODO: (可选) 通知房间内所有其他玩家房间已解散 (通过 WebSocket)
                // var otherPlayerUserIds = room.Players
                //                              .Where(p => p.UserId != requestingUserId)
                //                              .Select(p => p.UserId)
                //                              .ToList();
                // if (otherPlayerUserIds.Any())
                // {
                //     // await _notificationService.NotifyRoomDisbanded(otherPlayerUserIds, room.RoomId);
                // }

                _context.GameRooms.Remove(room); // EF Core 会处理级联删除 Players (如果配置正确)
                                                 // 如果没有配置级联删除，你可能需要手动删除 room.Players
                await _context.SaveChangesAsync();
                return new LeaveRoomResult { Success = true, Message = "房主离开，房间已解散。", RoomDisbanded = true };
            }
            else
            {
                // 普通玩家离开
                _context.Players.Remove(playerLeavingRecord); // 从 Players DbSet 中移除该玩家记录

                // TODO: (可选) 通知房间内其他玩家此人已离开 (通过 WebSocket)
                // var otherPlayerUserIds = room.Players
                //                              .Where(p => p.UserId != requestingUserId && p.Id != playerLeavingRecord.Id) // 排除刚离开的玩家
                //                              .Select(p => p.UserId)
                //                              .ToList();
                // if (otherPlayerUserIds.Any())
                // {
                //    // await _notificationService.NotifyPlayerLeft(otherPlayerUserIds, room.RoomId, playerLeavingRecord.User?.Username ?? "一位玩家");
                // }

                // 检查移除该玩家后房间是否变空（例如，房主先离开了，这是最后一个玩家）
                // 或者如果房间内只剩下房主，但房主不活跃等逻辑，可以根据业务需求添加
                // 当前逻辑：如果移除后房间没有其他玩家了（除了可能存在的房主，但房主若离开应走上面逻辑）
                // 注意：room.Players 集合在移除 playerLeavingRecord 后，在 SaveChangesAsync 之前可能还未更新。
                // 更可靠的检查是查看数据库或基于当前内存中的状态。
                var remainingPlayersCount = room.Players.Count(p => p.Id != playerLeavingRecord.Id);
                if (remainingPlayersCount == 0)
                {
                    _context.GameRooms.Remove(room); // 如果房间空了，也解散
                    await _context.SaveChangesAsync(); // 需要再次保存
                    return new LeaveRoomResult { Success = true, Message = "玩家离开，房间已空并解散。", RoomDisbanded = true };
                }


                await _context.SaveChangesAsync();
                return new LeaveRoomResult { Success = true, Message = "已成功离开房间。", RoomDisbanded = false };
            }
        }

        /// <summary>
        /// 用户离开游戏房间
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="playerId">要离开房间的玩家的 ID</param>
        /// <returns>离开成功返回 true，否则返回 false</returns>
        public async Task<bool> LeaveRoomAsync(int roomId, int playerId)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            var playerToRemove = gameRoom.Players.FirstOrDefault(p => p.Id == playerId);
            if (playerToRemove == null)
            {
                return false;
            }

            gameRoom.Players.Remove(playerToRemove);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceResponse> StartGameByRoomIdStringAsync(string roomIdString, int requestingUserId)
        {
            // 1. 查找房间，同时加载玩家信息以便进行权限验证
            var room = await _context.GameRooms 
                                     .Include(r => r.Players) // 加载房间内的所有 Player 记录
                                     .ThenInclude(p => p.User) // 对于每个 Player，加载其关联的 User 信息
                                     .FirstOrDefaultAsync(r => r.RoomId == roomIdString);

            if (room == null)
            {
                return new ServiceResponse { Success = false, Message = $"房间 '{roomIdString}' 不存在。" };
            }

            // 2. 检查房间当前状态
            if (room.Status != RoomStatus.Waiting)
            {
                string statusMessage = room.Status switch
                {
                    RoomStatus.Playing => "游戏已经进行中。",
                    RoomStatus.Completed => "游戏已结束，无法重新开始。",
                    RoomStatus.Closed => "房间已关闭。",
                    _ => "房间当前状态不允许开始游戏。"
                };
                return new ServiceResponse { Success = false, Message = statusMessage };
            }

            // 3. 权限验证：检查发起请求的用户是否为房主
            var hostPlayerRecord = room.Players.FirstOrDefault(p => p.IsHost);
            if (hostPlayerRecord == null || hostPlayerRecord.User == null || hostPlayerRecord.User.Id != requestingUserId)
            {
                // 如果找不到房主记录，或者房主记录没有关联用户，或者关联用户的ID与请求者ID不符
                return new ServiceResponse { Success = false, Message = "只有房主才能开始游戏。" };
            }

            // 4. (可选) 检查其他条件，例如最小玩家数
            if (room.Players.Count < 2)
            {
                return new ServiceResponse { Success = false, Message = "玩家人数不足（至少需要2人），无法开始游戏。" };
            }

            // 5. 更新游戏状态
            room.Status = RoomStatus.Playing; // 将状态设置为进行中
                                              // 在这里开始计时

            //_hubContext.//StartRoundWithTimer(roomIdString);




            // 在这里写调用后端计时器的方法 
            await _gameService.StartRoundTimer(roomIdString, GamePhase.DrawingAndGuessing, 75); //15秒选词，60秒作画




            // 1. 创建并初始化 ActiveGameState
            var activeGameState = new ActiveGameState
            {
                GameRoomId = room.Id, // 关联到当前 GameRoom 的主键 Id
                TotalRounds = room.Rounds, // 使用 GameRoom 配置的回合数作为总轮数
                                           // 或者根据玩家人数: room.Players.Count; (你需要决定哪个优先)
                CurrentRound = 0, // 将在 StartNewRound (我们稍后会创建的方法) 中设为 1
                CurrentGamePhase = GamePhase.NotStarted, // 将在 StartNewRound 中更新
                PlayerScoresJson = JsonSerializer.Serialize(new Dictionary<int, int>()) // 初始化空得分
                // CurrentPainterUserId 和 CurrentTargetWord 将在 StartNewRound 中设置
            };
            // 将 ActiveGameState 添加到上下文并准备保存
            _context.ActiveGameStates.Add(activeGameState);

            //将 ActiveGameState 实例关联回 GameRoom 实体
            // 这样如果后续立即访问 room.ActiveState，它不是 null (尽管EF Core在下次查询时会填充它)
            room.ActiveState = activeGameState;
            
            try
            {
                //_context.GameRooms.Update(room); // 标记实体为已修改 (对于跟踪的实体，EF Core 通常会自动检测变化)
                await _context.SaveChangesAsync(); // 保存更改到数据库
                // TODO: 在这里调用一个方法来开始游戏的第一轮逻辑, 例如:
                await StartFirstRoundAsync(activeGameState, room.Players);
                // 这个方法会负责选择第一个画师，生成选词等，并通过SignalR通知客户端。
                // 我们将在下一步实现这个游戏逻辑的起点。
                return new ServiceResponse { Success = true, Message = "游戏已成功开始。" };
            }
            catch (DbUpdateException ex) // 更具体的异常捕获
            {
                // 记录详细错误，包括内部异常
                Console.WriteLine($"Error starting game (DbUpdateException) (RoomIdString: {roomIdString}): {ex.ToString()}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.ToString()}");
                }
                return new ServiceResponse { Success = false, Message = "开始游戏时发生数据库错误，请稍后重试。" };
            }
            catch (Exception ex)
            {
                // 在实际应用中，这里应该使用更完善的日志记录机制
                Console.WriteLine($"Error starting game (RoomIdString: {roomIdString}): {ex.ToString()}");
                return new ServiceResponse { Success = false, Message = "开始游戏时发生数据库错误，请稍后重试。" };
            }
        }

        private async Task StartFirstRoundAsync(ActiveGameState activeGameState, List<Player> playersInRoom)
        {
            // TODO: 实现开始第一轮的逻辑
            // 1. 验证 activeGameState 和 playersInRoom 是否有效
            if (activeGameState == null || playersInRoom == null || !playersInRoom.Any())
            {
                Console.WriteLine($"[StartFirstRoundAsync] Error: Invalid activeGameState or no players in room (GameRoomId: {activeGameState?.GameRoomId}).");
                // 可能需要记录更详细的错误或抛出异常
                return;
            }

            // 2. 设置当前回合为第一回合
            activeGameState.CurrentRound = 1;

            // 3. 选择第一个画师 (简单示例：选择列表中的第一个玩家)
            //    在实际应用中，你可能需要更复杂的逻辑，例如随机选择或按顺序轮流
            var firstPainter = playersInRoom.FirstOrDefault();
            if (firstPainter?.User == null) // 确保玩家及其关联的 User 对象存在
            {
                Console.WriteLine($"[StartFirstRoundAsync] Error: Could not determine the first painter (GameRoomId: {activeGameState.GameRoomId}).");
                activeGameState.CurrentGamePhase = GamePhase.GameOver; // 或者一个错误状态
                await _context.SaveChangesAsync();
                return;
            }
            activeGameState.CurrentPainterUserId = firstPainter.User.Id;

            // 4. 为画师生成词语选项 (从数据库获取)
            // 4.1 获取当前房间配置的词库分类
            var gameRoomForCategories = await _context.GameRooms.FindAsync(activeGameState.GameRoomId);
            if (gameRoomForCategories == null)
            {
                Console.WriteLine($"[StartFirstRoundAsync] 错误: 找不到 ID 为 {activeGameState.GameRoomId} 的 GameRoom 以获取分类信息。");
                activeGameState.CurrentGamePhase = GamePhase.GameOver; // 或错误状态
                activeGameState.WordChoicesForPainter = new List<string> { "错误", "词库", "无法", "加载" }; // 备用词
                await _context.SaveChangesAsync();
                return;
            }
            List<string> roomCategories = gameRoomForCategories.Categories;

            if (roomCategories == null || !roomCategories.Any())
            {
                Console.WriteLine($"[StartFirstRoundAsync] 警告: 房间 {gameRoomForCategories.RoomId} 没有选择任何词库分类。使用默认备用词。");
                activeGameState.WordChoicesForPainter = new List<string> { "默认A", "默认B", "默认C", "默认D" };
            }
            else
            {
                // 4.2 从数据库中根据分类随机抽取词语
                // 确保你的 OurDbContext 中有 DbSet<Word> Words
                var wordsQuery = _context.Words.Where(w => w.Category != null && roomCategories.Contains(w.Category));

                // 首先统计符合条件的词语数量，以处理词语较少的情况
                int availableWordsCount = await wordsQuery.CountAsync();
                List<string> selectedWords;

                if (availableWordsCount == 0)
                {
                    Console.WriteLine($"[StartFirstRoundAsync] 错误: 在房间 {gameRoomForCategories.RoomId} 中，找不到分类为 {string.Join(", ", roomCategories)} 的词语。使用备用词。");
                    selectedWords = new List<string> { "无词A", "无词B", "无词C", "无词D" };
                }
                else
                {
                    // 如果可用词语少于4个，则全部选取；否则，随机选取4个。
                    int wordsToTake = Math.Min(4, availableWordsCount);

                    selectedWords = await wordsQuery
                                            .OrderBy(w => Guid.NewGuid()) // 简单随机化方法，对于非常大的表可能性能不佳
                                            .Take(wordsToTake)
                                            .Select(w => w.Content) // 选择 Word 模型的 'Content' 属性
                                            .Where(content => content != null) // 确保 Content 不为 null
                                            .ToListAsync();
                    
                    // 如果选取后得到的词语列表为空（例如，所有匹配词的 Content 都为 null）
                    if (!selectedWords.Any() && availableWordsCount > 0) {
                        Console.WriteLine($"[StartFirstRoundAsync] 警告: 选取后词语列表为空 (可能所有 Content 都为 null)。房间: {gameRoomForCategories.RoomId}. 使用备用词。");
                        selectedWords = new List<string> { "备用词1", "备用词2", "备用词3", "备用词4" };
                    }

                }
                activeGameState.WordChoicesForPainter = selectedWords!; // 分配选取的词语（或备用词）
            }


            // 5. 设置游戏阶段为等待画师选词
            activeGameState.CurrentGamePhase = GamePhase.WaitingForPainterToChooseWord;

            // 6. 保存对 activeGameState 的更改
            try
            {
                // _context.ActiveGameStates.Update(activeGameState); // EF Core 会跟踪已加载实体的变化
                await _context.SaveChangesAsync();
                Console.WriteLine($"[StartFirstRoundAsync] 第一轮已为 GameRoomId: {activeGameState.GameRoomId} 初始化。画师: {activeGameState.CurrentPainterUserId}。阶段: {activeGameState.CurrentGamePhase}。可选词语: {string.Join(", ", activeGameState.WordChoicesForPainter ?? new List<string>())}");

                // 7. 通过 SignalR 通知客户端游戏状态已更新
                try
                {
                    // 获取与 ActiveGameState 关联的 GameRoom 的字符串 RoomId (用于 SignalR 组名)
                    var gameRoom = await _context.GameRooms.FindAsync(activeGameState.GameRoomId);
                    if (gameRoom == null || string.IsNullOrEmpty(gameRoom.RoomId))
                    {
                        Console.WriteLine($"[StartFirstRoundAsync] Error: Could not find GameRoom or RoomId for ActiveGameStateId: {activeGameState.Id} to send SignalR message.");
                        return;
                    }

                    // 准备要发送给客户端的游戏状态数据
                    // 你可能需要创建一个 DTO (Data Transfer Object) 来精确控制发送给客户端的数据结构
                    // 为了简单起见，我们直接发送 ActiveGameState，但要注意 WordChoicesForPainter 只应发送给画师
                    var gameStateForClients = new
                    {
                        activeGameState.CurrentRound,
                        activeGameState.TotalRounds,
                        activeGameState.CurrentPainterUserId,
                        // CurrentTargetWord 此时应该是 null 或空，因为画师还没选
                        activeGameState.CurrentGamePhase,
                        // PlayerScoresJson 可以发送，前端可以解析显示
                        //activeGameState.PlayerScoresJson,
                        // WordChoicesForPainter 需要特殊处理
                    };

                    // 向房间内的所有客户端广播游戏状态更新
                    // 前端需要监听 "GameStateUpdated" 事件
                    await _hubContext.Clients.Group(gameRoom.RoomId).SendAsync("GameStateUpdated", gameStateForClients);
                    Console.WriteLine($"[SignalR] Sent 'GameStateUpdated' to group: {gameRoom.RoomId}");

                    // 单独向画师发送可选的词语列表
                    if (activeGameState.CurrentPainterUserId.HasValue)
                    {
                        // 我们需要找到画师的 ConnectionId。这通常在 GameHub 的 JoinRoom 时映射。
                        // 从服务层直接获取 ConnectionId 比较困难，通常 Hub 层更适合处理这种针对特定连接的消息。
                        // 方案1: Hub 在 JoinRoom 时将 UserId -> ConnectionId 映射存储起来，服务层查询这个映射 (需要共享存储或服务)。
                        // 方案2: 服务层触发一个事件，Hub 监听并处理。
                        // 方案3 (更常见): Hub 层调用服务层方法后，Hub 层自己负责发送这些特定消息。
                        // 方案4 (简单但不完美): 广播词语列表，前端画师客户端根据 isPainter 决定是否处理。
                        //
                        // 为了简单起见，我们暂时也广播给整个组，前端画师根据 isPainter 标志来显示词语选择。
                        // 或者，我们可以定义一个特定的消息只包含词语选项，由画师客户端监听。
                        // 例如:
                        await _hubContext.Clients.Group(gameRoom.RoomId).SendAsync("WordChoicesAvailable", new
                        {
                            PainterUserId = activeGameState.CurrentPainterUserId,
                            Choices = activeGameState.WordChoicesForPainter
                        });
                        Console.WriteLine($"[SignalR] Sent 'WordChoicesAvailable' to group: {gameRoom.RoomId} for painter: {activeGameState.CurrentPainterUserId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[StartFirstRoundAsync] Error sending SignalR messages for GameRoomId: {activeGameState.GameRoomId}. Exception: {ex.ToString()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StartFirstRoundAsync] Error saving state or notifying clients for GameRoomId: {activeGameState.GameRoomId}. Exception: {ex.ToString()}");
                // 处理保存失败的情况
            }
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <returns>开始成功返回 true，否则返回 false</returns>
        public async Task<bool> StartGameAsync(int roomId)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null || gameRoom.Status != RoomStatus.Waiting)
            {
                return false;
            }

            gameRoom.Status = RoomStatus.Playing;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <returns>结束成功返回 true，否则返回 false</returns>
        public async Task<bool> EndGameAsync(int roomId)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null || gameRoom.Status != RoomStatus.Playing)
            {
                return false;
            }

            gameRoom.Status = RoomStatus.Completed;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <returns>暂停成功返回 true，否则返回 false</returns>
        public async Task<bool> PauseGameAsync(int roomId)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.Status != RoomStatus.Playing)
            {
                return false;
            }

            gameRoom.Status = RoomStatus.Waiting;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <returns>恢复成功返回 true，否则返回 false</returns>
        public async Task<bool> ResumeGameAsync(int roomId)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.Status != RoomStatus.Waiting)
            {
                return false;
            }

            gameRoom.Status = RoomStatus.Playing;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 设置游戏房间的游戏配置
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="gameConfig">新的游戏配置对象</param>
        /// <returns>设置成功返回 true，否则返回 false</returns>
        public async Task<bool> SetGameConfigAsync(int roomId, GameConfig gameConfig)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.GameConfig = gameConfig;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 在游戏房间内发送聊天消息
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="chatMessage">要发送的聊天消息对象</param>
        /// <returns>发送成功返回 true，否则返回 false</returns>
        public async Task<bool> SendChatMessageAsync(int roomId, ChatMessage chatMessage)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.ChatHistory.Add(chatMessage);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 设置游戏房间的私密状态
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="isPrivate">是否私密，true 表示私密，false 表示公开</param>
        /// <returns>设置成功返回 true，否则返回 false</returns>
        public async Task<bool> SetPrivateStatusAsync(int roomId, bool isPrivate)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.IsPrivate = isPrivate;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 设置游戏房间的密码
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="password">新的房间密码</param>
        /// <returns>设置成功返回 true，否则返回 false</returns>
        public async Task<bool> SetRoomPasswordAsync(int roomId, string password)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.RoomPassword = password;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 设置游戏房间的最大玩家数
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="maxPlayers">最大玩家数</param>
        /// <returns>设置成功返回 true，否则返回 false</returns>
        public async Task<bool> SetMaxPlayersAsync(int roomId, int maxPlayers)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null || maxPlayers <= 0)
            {
                return false;
            }

            gameRoom.MaxPlayers = maxPlayers;
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// 更新游戏房间信息
        /// </summary>
        /// <param name="roomData">要更新的游戏房间对象</param>
        /// <returns>更新成功返回更新后的游戏房间对象，否则返回 null</returns>
        public async Task<GameRoom?> UpdateRoomAsync(GameRoom roomData)
        {
            var existingRoom = await _context.GameRooms
                .Include(gr => gr.Players)
                .Include(gr => gr.ChatHistory)
                .FirstOrDefaultAsync(gr => gr.Id == roomData.Id);

            if (existingRoom == null)
            {
                return null;
            }

            // 更新房间的基本信息
            existingRoom.Name = roomData.Name;
            existingRoom.Status = roomData.Status;
            existingRoom.GameMode = roomData.GameMode;
            existingRoom.IsPrivate = roomData.IsPrivate;
            existingRoom.RoomPassword = roomData.RoomPassword;
            existingRoom.MaxPlayers = roomData.MaxPlayers;
            existingRoom.Rounds = roomData.Rounds;
            existingRoom.Categories = roomData.Categories;
            existingRoom.GameConfig = roomData.GameConfig;

            // 你可能还需要更新关联的玩家和聊天记录等信息，这里仅做简单示例
            // 例如，如果你需要更新玩家列表，可以添加相应的逻辑

            try
            {
                await _context.SaveChangesAsync();
                return existingRoom;
            }
            catch (Exception ex)
            {
                // 在实际应用中，这里应该使用更完善的日志记录机制
                Console.WriteLine($"Error updating game room (RoomId: {roomData.Id}): {ex.ToString()}");
                return null;
            }
        }
        public class LeaveRoomResult
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public bool RoomDisbanded { get; set; } = false;
        }
        public class ServiceResponse // 请确保这个类只定义一次
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }
}