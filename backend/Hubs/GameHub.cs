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
    }
}
