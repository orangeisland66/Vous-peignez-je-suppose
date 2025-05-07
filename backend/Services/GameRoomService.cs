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
        public async Task<List<GameRoom>> GetAllGameRoomsAsync()
        {
            return await _context.GameRooms.ToListAsync();
        }

        /// <summary>
        /// 根据房间 ID 获取游戏房间
        /// </summary>
        /// <param name="roomId">游戏房间的 ID</param>
        /// <returns>对应的游戏房间对象，如果不存在则返回 null</returns>
        public async Task<GameRoom> GetGameRoomByIdAsync(int roomId)
        {
            return await _context.GameRooms
               .Include(gr => gr.players)
               .Include(gr => gr.chatHistory)
               .FirstOrDefaultAsync(gr => gr.id == roomId);
        }

        /// <summary>
        /// 用户加入游戏房间
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="user">要加入房间的用户</param>
        /// <param name="password">房间密码（如果房间是私密的）</param>
        /// <returns>加入成功返回 true，否则返回 false</returns>
        public async Task<bool> JoinRoomAsync(int roomId, User user, string password = null)
        {
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.isPrivate && gameRoom.roomPassword!= password)
            {
                return false;
            }

            // 检查用户是否已在房间内
            if (gameRoom.players.Any(p => p.id == user.id))
            {
                return false;
            }

            var player = new Player { id = user.id, username = user.username, role = "未分配" };
            gameRoom.players.Add(player);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 用户离开游戏房间
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        /// <param name="user">要离开房间的用户</param>
        /// <returns>离开成功返回 true，否则返回 false</returns>
        public async Task<bool> LeaveRoomAsync(int roomId, User user)
        {
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            var playerToRemove = gameRoom.players.FirstOrDefault(p => p.id == user.id);
            if (playerToRemove == null)
            {
                return false;
            }

            gameRoom.players.Remove(playerToRemove);
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.status!= RoomStatus.Waiting)
            {
                return false;
            }
            {
                return false;
            }

            // 可以在这里添加开始游戏的其他逻辑，例如初始化游戏配置等
            gameRoom.status = RoomStatus.Playing;
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.status!= RoomStatus.Playing)
            {
                return false;
            }
            {
                return false;
            }

            // 可以在这里添加结束游戏的其他逻辑，例如保存游戏结果等
            gameRoom.status = RoomStatus.Closed;
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.status!= RoomStatus.Playing)
            {
                return false;
            }
            {
                return false;
            }
            {
                return false;
            }

            gameRoom.status =RoomStatus.Waiting;
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            if (gameRoom.status!= RoomStatus.Waiting)
            {
                return false;
            }

            gameRoom.status = RoomStatus.Playing;
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.gameConfig = gameConfig;
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.chatHistory.Add(chatMessage);
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.isPrivate = isPrivate;
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
            var gameRoom = await GetGameRoomByIdAsync(roomId);
            if (gameRoom == null)
            {
                return false;
            }

            gameRoom.roomPassword = password;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<GameRoom> GetRoomDetailsAsync(int roomId)
        {
            return await _context.GameRooms
               .Include(gr => gr.players)
               .Include(gr => gr.chatHistory)
               .FirstOrDefaultAsync(gr => gr.id == roomId);
        }

    }
}