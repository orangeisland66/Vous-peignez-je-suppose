using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;

namespace backend.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameRoomService _gameRoomService;
        private readonly GameService _gameService;
        private readonly Dictionary<string, int> _connectionPlayerMap;

        public GameHub(GameRoomService gameRoomService, GameService gameService)
        {
            _gameRoomService = gameRoomService;
            _gameService = gameService;
            _connectionPlayerMap = new Dictionary<string, int>();
        }

        // 玩家加入房间
        public async Task JoinRoom(int roomId, Player player)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("RoomNotFound", "房间不存在");
                return;
            }

            var result = await _gameRoomService.JoinRoomAsync(roomId, player);
            if (result)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                _connectionPlayerMap[Context.ConnectionId] = player.Id;
                await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", player.Username);
                await Clients.Caller.SendAsync("JoinedRoom", roomId);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", "加入房间失败");
            }
        }

        //玩家退出登录
        public async Task Logout(int roomId)
        {
            if (_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            {
                _connectionPlayerMap.Remove(Context.ConnectionId);
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
            var roomId = GetRoomIdFromContext();
            if (roomId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                _connectionPlayerMap.Remove(Context.ConnectionId);
                await Clients.Group(roomId.ToString()).SendAsync("PlayerLeft", Context.ConnectionId);
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
        public async Task SendChatMessage(int roomId, string message)
        {
            Console.WriteLine("111");
            if (string.IsNullOrWhiteSpace(message)) return;

            // 从连接映射中获取玩家 ID（确保已加入房间）
            // if (!_connectionPlayerMap.TryGetValue(Context.ConnectionId, out int playerId))
            // {
            //     await Clients.Caller.SendAsync("NotAuthorized", "未找到玩家信息");
            //     return;
            // }
            // 测试用：硬编码玩家 ID（玩家一的 ID 假设为 1）
            int playerId = 1; // 需确保数据库中存在 ID 为 1 的玩家

            // 假设从数据库获取玩家用户名（测试时可固定为已知用户名）
            var player = new Player { Id = playerId, Username = "玩家一" }; // 临时模拟玩家对象


            // 假设从服务中获取玩家用户名（需根据实际业务调整）
            // var player = await _gameRoomService.GetPlayerByIdAsync(playerId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("PlayerNotFound", "玩家信息不存在");
                return;
            }

            // 构建聊天消息实体（包含时间戳）
            var chatMessage = new ChatMessage
            {
                Content = message,
                SenderId = playerId,
                GameRoomId = roomId,
                Timestamp = DateTime.UtcNow // 使用 UTC 时间
            };

            // 保存到数据库
            await _gameRoomService.SendChatMessageAsync(roomId, chatMessage);

            // 广播消息给房间内所有玩家（字段需与前端匹配）
            // await Clients.Group(roomId.ToString()).SendAsync("ReceiveChatMessage", new 
            await Clients.All.SendAsync("ReceiveChatMessage", new
            {
                playerId,
                username = player.Username, // 玩家用户名
                content = message, // 消息内容（前端字段为 content）
                timestamp = chatMessage.Timestamp.ToString("o") // ISO 8601 格式时间戳
            });
            Console.WriteLine($"Chat message sent from player {playerId} in room {roomId}: {message}");
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
    }
}

