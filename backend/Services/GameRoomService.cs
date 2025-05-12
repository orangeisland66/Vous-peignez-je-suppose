using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;

namespace backend.Services
{
    public class GameRoomService
    {
        private readonly OurDbContext _context;

        public GameRoomService(OurDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 创建游戏房间
        /// </summary>
        /// <param name="gameRoom">要创建的游戏房间对象</param>
        /// <returns>创建成功的游戏房间对象</returns>
        public async Task<GameRoom> CreateRoomAsync(GameRoom gameRoom)
        {
            _context.GameRooms.Add(gameRoom);
            await _context.SaveChangesAsync();
            return gameRoom;
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
                .Include(gr => gr.ChatHistory)
                .FirstOrDefaultAsync(gr => gr.Id == roomId);
        }

        /// <summary>
        /// 用户加入游戏房间
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="player">要加入房间的玩家</param>
        /// <returns>加入成功返回 true，否则返回 false</returns>
        public async Task<bool> JoinRoomAsync(int roomId, Player player)
        {
            var gameRoom = await GetRoomDetailsAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.IsPrivate && gameRoom.RoomPassword != player.Password)
            {
                return false;
            }

            if (gameRoom.Players.Any(p => p.Id == player.Id))
            {
                return false;
            }

            gameRoom.Players.Add(player);
            await _context.SaveChangesAsync();
            return true;
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
        
    }
}