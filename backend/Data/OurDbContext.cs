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
        public DbSet<GameRoundScore> GameRoundScores { get; set; }
        public DbSet<GameHistory> GameHistories { get; set; }
        public DbSet<PlayerScore> PlayerScores { get; set; }


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

            modelBuilder.Entity<GameRoom>()
                .HasMany(gr => gr.Players)  // GameRoom 有多个 Players
                .WithOne()                  // Player 没有反向导航属性
                .HasForeignKey(p => p.GameRoomId) // 外键指向 GameRoomId
                .OnDelete(DeleteBehavior.Cascade); // 可选：删除房间时级联删除玩家

            modelBuilder.Entity<GameRoom>()
            .HasMany(gr => gr.ChatHistory)    // GameRoom 拥有多个 ChatMessage
            .WithOne()                        // ChatMessage 没有反向导航到 GameRoom
            .HasForeignKey(cm => cm.GameRoomId) // 外键
            .OnDelete(DeleteBehavior.Cascade);  // 可选：级联删除

            modelBuilder.Entity<GameRoom>()
            .OwnsOne(r => r.GameConfig);
            // 配置Game实体
            modelBuilder.Entity<Game>()
                .HasOne(g => g.GameRoom)
                .WithMany()
                .HasForeignKey(g => g.GameRoomId);

            modelBuilder.Entity<GameHistory>(entity =>
            {
                entity.HasKey(h => h.Id);
                
                // 配置与PlayerScore的一对多关系
                entity.HasMany(h => h.Scores)
                    .WithOne()
                    .HasForeignKey(s => s.GameHistoryId)
                    .OnDelete(DeleteBehavior.Cascade);
             });
            // 配置DrawingData实体
            modelBuilder.Entity<DrawingData>()
                .HasOne(d => d.Game)
                .WithMany()
                .HasForeignKey(d => d.GameId);

            modelBuilder.Entity<DrawingData>()
                .HasOne(d => d.Player)
                .WithMany()
                .HasForeignKey(d => d.PlayerId);

            modelBuilder.Entity<GameRoundScore>()
                .HasOne(grs => grs.Game)
                .WithMany(g => g.RoundScores)
                .HasForeignKey(grs => grs.GameId);

            modelBuilder.Entity<PlayerScore>(entity =>
            {
                entity.HasKey(s => s.Id);
                
                // 配置与User的关系
                entity.HasOne(s => s.Player)
                    .WithMany()
                    .HasForeignKey(s => s.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // 配置分数属性
                entity.Property(s => s.Score).IsRequired();
            });
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}