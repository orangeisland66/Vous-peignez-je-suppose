using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class OurDbContext : DbContext
    {
        public OurDbContext(DbContextOptions<OurDbContext> options)
            : base(options)
        {
        }

        // 定义各个实体的集合
        public DbSet<User> Users { get; set; }
        public DbSet<GameRoom> GameRooms { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<DrawingData> DrawingData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置User实体
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 配置GameRoom实体
            modelBuilder.Entity<GameRoom>()
                .HasMany(gr => gr.Players)
                .WithOne(p => p.GameRoom)
                .HasForeignKey(p => p.GameRoomId);

            modelBuilder.Entity<GameRoom>()
                .HasMany(gr => gr.ChatHistory)
                .WithOne(cm => cm.GameRoom)
                .HasForeignKey(cm => cm.GameRoomId);

            // 配置Game实体
            modelBuilder.Entity<Game>()
                .HasOne(g => g.GameRoom)
                .WithMany()
                .HasForeignKey(g => g.GameRoomId);

            // 配置DrawingData实体
            modelBuilder.Entity<DrawingData>()
                .HasOne(d => d.Game)
                .WithMany()
                .HasForeignKey(d => d.GameId);

            modelBuilder.Entity<DrawingData>()
                .HasOne(d => d.Player)
                .WithMany()
                .HasForeignKey(d => d.PlayerId);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}