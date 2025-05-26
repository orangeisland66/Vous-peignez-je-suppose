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
        public DbSet<ActiveGameState> ActiveGameStates { get; set; }


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
            .WithOne(p => p.GameRoom)   // Player 属于一个 GameRoom,添加反向导航属性
            .HasForeignKey(p => p.GameRoomId) // 外键指向 GameRoomId
            .HasPrincipalKey(gr => gr.RoomId) // 主键使用 GameRoom 的 RoomId
            .OnDelete(DeleteBehavior.Cascade); // 可选：删除房间时级联删除玩家

            // **新增：配置 GameRoom 与 Creator (User) 的关系 (一对多，一个 User 可以创建多个 GameRoom)**
            modelBuilder.Entity<GameRoom>()
            .HasOne(gr => gr.Creator) // GameRoom 有一个 Creator
            .WithMany() // User 模型没有反向导航到它创建的 GameRoom 列表
            .HasForeignKey(gr => gr.CreatorId) // GameRoom 中的外键是 CreatorId
            .OnDelete(DeleteBehavior.Restrict); // 或 .ClientSetNull，当删除用户时，其创建的房间的 CreatorId 不会级联删除房间，而是可能设为 null 或报错，取决于你的业务逻辑。Restrict 通常更安全。

            // --- 配置 GameRoom 和 ActiveGameState 之间的一对一关系 ---
            // 一个 GameRoom 可以有一个 ActiveGameState (当游戏进行时)
            // 一个 ActiveGameState 必须属于一个 GameRoom
            modelBuilder.Entity<GameRoom>()
            .HasOne(g => g.ActiveState)         // GameRoom 有一个 ActiveState
            .WithOne(a => a.GameRoom)           // ActiveState 有一个 GameRoom
            .HasForeignKey<ActiveGameState>(a => a.GameRoomId) // ActiveGameState 中的外键是 GameRoomId
            .OnDelete(DeleteBehavior.Cascade); // 当 GameRoom 被删除时，关联的 ActiveGameState 也被删除 (可选行为)

            modelBuilder.Entity<GameRoom>()
            .HasMany(gr => gr.ChatHistory)    // GameRoom 拥有多个 ChatMessage
            .WithOne()                        // ChatMessage 没有反向导航到 GameRoom
            .HasForeignKey(cm => cm.GameRoomId) // 外键
            .OnDelete(DeleteBehavior.Cascade);  // 可选：级联删除

            modelBuilder.Entity<GameRoom>()
            .OwnsOne(r => r.GameConfig);

            // 配置 Player 与 User 的关系 (多对一，多个 Player 记录可以指向同一个 User)
            // 这个配置应该在 Player 实体定义时考虑，或者如果 Player.User 是主要导航
            modelBuilder.Entity<Player>()
            .HasOne(p => p.User)
            .WithMany() // User 模型没有 ICollection<Player> PlayerInstances 这样的导航
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); // 如果删除了用户，其相关的 Player 记录也删除 (需要仔细考虑业务逻辑)


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

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.GameRoom)
                .WithMany(gr => gr.ChatHistory)
                .HasForeignKey(cm => cm.GameRoomId)
                .HasPrincipalKey(gr => gr.RoomId)  // 使用 RoomId 作为主键
                .OnDelete(DeleteBehavior.Cascade);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}