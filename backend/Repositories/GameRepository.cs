using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Repositories.GameRepository
{
    public class GameRepository
    {
        private readonly OurDbContext _context;

        public GameRepository(OurDbContext context)
        {
            _context = context;
        }

        // 异步根据 ID 获取游戏房间
        public async Task<GameRoom> GetRoomByIdAsync(int id)
        {
            return await _context.GameRooms.FindAsync(id);
        }

        // 异步创建游戏房间
        public async Task<GameRoom> CreateRoomAsync(GameRoom room)
        {
            _context.GameRooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        // 异步更新游戏房间
        public async Task UpdateRoomAsync(GameRoom room)
        {
            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // 异步删除游戏房间
        public async Task DeleteRoomAsync(int id)
        {
            var room = await _context.GameRooms.FindAsync(id);
            if (room != null)
            {
                _context.GameRooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
    }
}