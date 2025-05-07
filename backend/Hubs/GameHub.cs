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

        public GameHub(GameRoomService gameRoomService, GameService gameService)
        {
            _gameRoomService = gameRoomService;
            _gameService = gameService;
        }

        // 玩家加入房间时，通知房间内的其他玩家
        public async Task JoinRoom(int roomId, Player player)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("RoomNotFound", "房间不存在");
                return;
            }

            // 将玩家加入房间
            var result = await _gameRoomService.JoinRoomAsync(roomId, player);
            if (result.IsSuccess)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", player.Username);
                await Clients.Caller.SendAsync("JoinedRoom", roomId);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", result.ErrorMessage);
            }
        }

        // 玩家开始作画
        public async Task PlayerDraw(int roomId, Stroke stroke)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法作画");
                return;
            }

            // 通知房间内其他玩家
            await Clients.Group(roomId.ToString()).SendAsync("PlayerDrawing", stroke);
        }

        // 玩家进行猜词
        public async Task PlayerGuess(int roomId, string guessedWord)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法猜词");
                return;
            }

            // 通知房间内其他玩家
            var result = await _gameService.CheckGuessAsync(roomId, guessedWord);
            if (result.Correct)
            {
                await Clients.Group(roomId.ToString()).SendAsync("CorrectGuess", guessedWord);
            }
            else
            {
                await Clients.Group(roomId.ToString()).SendAsync("IncorrectGuess", guessedWord);
            }
        }

        // 开始游戏时，广播游戏开始的消息
        public async Task StartGame(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.status != RoomStatus.Waiting)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "房间状态无法开始游戏");
                return;
            }

            await Clients.Group(roomId.ToString()).SendAsync("GameStarted", "游戏开始了！");
        }

        // 切换到下一个轮次时，通知所有玩家
        public async Task NextRound(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.status != RoomStatus.Playing)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法进入下一轮");
                return;
            }

            await Clients.Group(roomId.ToString()).SendAsync("NextRound", "进入下一轮！");
        }

        // 游戏结束时，广播最终得分
        public async Task EndGame(int roomId, Dictionary<string, int> scores)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.status != RoomStatus.Completed)
            {
                await Clients.Caller.SendAsync("InvalidRoomStatus", "当前房间无法结束游戏");
                return;
            }

            await Clients.Group(roomId.ToString()).SendAsync("GameEnded", scores);
        }

        // 断开连接时，移除玩家
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var roomId = GetRoomIdFromContext();
            if (roomId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("PlayerLeft", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // 获取当前连接的房间 ID
        private int? GetRoomIdFromContext()
        {
            // 假设通过一些上下文信息获取房间 ID，可以根据实际情况修改
            // 例如，可以使用 Context.Items 或其他方式来存储房间信息
            return null;
        }
    }
}
