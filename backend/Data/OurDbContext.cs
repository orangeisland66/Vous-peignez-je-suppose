using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class OurDbContext : DbContext
    {
        public OurDbContext(DbContextOptions<OurDbContext> options) : base(options)
        {
        }

        // 定义各个实体的集合
        public DbSet<User> Users { get; set; }
        public DbSet<GameRoom> GameRooms { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<Game> Games { get; set; }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}