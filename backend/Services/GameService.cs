using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.SignalR;
using backend.Hubs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Services
{
    public class GameService
    {
        private readonly OurDbContext _context;
        private readonly WordManager _wordManager;

        private readonly IHubContext<GameHub> _hubContext;


        // TimerInfo 内部类
        private class TimerInfo
        {
            public string RoomId { get; set; }
            public GamePhase Phase { get; set; }
            public Stopwatch Stopwatch { get; set; } = new Stopwatch();//使用更加精确的计时器
            public DateTime StartTime { get; set; }
            public int DurationSeconds { get; set; }
            public int RemainingSeconds { get; set; }
            public Timer Timer { get; set; }

            //计算剩余时间
            public double GetRemainingSeconds()
            {
                if (!Stopwatch.IsRunning)
                    return 0;

                var elapsedSeconds = Stopwatch.Elapsed.TotalSeconds;
                return Math.Max(0, DurationSeconds - elapsedSeconds);
            }
        }


        //计时器相关
        static private readonly ConcurrentDictionary<string, TimerInfo> _activeTimers = new();

        public GameService(OurDbContext context, WordManager wordManager, IHubContext<GameHub> hubContext)
        {

            _context = context;
            _wordManager = wordManager;
            _hubContext = hubContext;
        }


        // 优化版本 StartRoundTimer 方法
        public async Task StartRoundTimer(string roomId, GamePhase phase, int durationSeconds)
        {
            // 停止现有计时器
            await StopTimer(roomId);

            var timerInfo = new TimerInfo
            {
                RoomId = roomId,
                Phase = phase,
                DurationSeconds = durationSeconds
            };

            // 启动 Stopwatch
            timerInfo.Stopwatch.Start();

            // 创建定时器，每秒触发一次
            timerInfo.Timer = new Timer(OnTimerTick, roomId, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            _activeTimers[roomId] = timerInfo;

            Console.WriteLine($"房间{roomId}的计时器已启动，持续时间: {durationSeconds} 秒");

            // 立即发送初始时间
            await _hubContext.Clients.Group(roomId).SendAsync("TimerUpdate", durationSeconds);
        }

        //  优化版本 修改 StopTimer 方法
        public async Task StopTimer(string roomId)
        {
            Console.WriteLine($"尝试停止房间{roomId}的计时器");
            if (_activeTimers.TryRemove(roomId, out var timerInfo))
            {
                // 停止计时器
                timerInfo.Timer?.Change(Timeout.Infinite, Timeout.Infinite); // 停止触发
                timerInfo.Timer?.Dispose(); // 释放资源
                timerInfo.Timer = null;

                // 停止 Stopwatch
                timerInfo.Stopwatch?.Stop();
                timerInfo.Stopwatch = null;

                Console.WriteLine($"房间{roomId}的计时器已停止");

                // 通知前端计时器已停止
                await _hubContext.Clients.Group(roomId).SendAsync("TimerStopped");
            }
        }
        

        // 优化版本 OnTimerTick 方法
        private async void OnTimerTick(object state)
        {
            var roomId = (string)state;

            if (!_activeTimers.TryGetValue(roomId, out var timerInfo))
                return;

            // 获取精确的剩余时间
            var remainingSeconds = timerInfo.GetRemainingSeconds();

            // 向上取整发送给前端（用户友好）
            var displaySeconds = (int)Math.Round(remainingSeconds);

            Console.WriteLine($"现在在GameService的OnTimerTick方法中,将广播时间到前端，剩余时间: {remainingSeconds:F2} 秒，显示: {displaySeconds} 秒");

            // 广播到前端
            await _hubContext.Clients.Group(roomId).SendAsync("TimerUpdate", displaySeconds);

            // 检查是否时间到了
            if (remainingSeconds <= 0)
            {
                Console.WriteLine($"房间{roomId}的计时器结束");
                await StopTimer(roomId);

                // 可以在这里添加回合结束的逻辑
                await _hubContext.Clients.Group(roomId).SendAsync("TimerCompleted");
            }
        }



        /// <summary>
        /// 开始游戏轮次
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <returns>如果成功开始游戏轮次返回 true，否则返回 false</returns>
        public async Task<bool> StartRoundAsync(int gameId)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != GameStatus.Playing)
            {
                return false;
            }

            // 选择下一个作画玩家
            var nextPlayer = game.Players
                .OrderBy(p => p.LastDrawingTime ?? DateTime.MinValue)
                .FirstOrDefault();

            if (nextPlayer == null)
            {
                return false;
            }

            // 更新游戏状态
            game.CurrentDrawingPlayerId = nextPlayer.Id;
            game.CurrentWord = await GetRandomWordAsync();
            game.GameStartedAt = DateTime.UtcNow;
            nextPlayer.LastDrawingTime = DateTime.UtcNow;

            // 重置所有玩家的猜测状态
            foreach (var player in game.Players)
            {
                player.HasGuessed = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 结束游戏轮次
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <returns>如果成功结束游戏轮次返回 true，否则返回 false</returns>
        public async Task<bool> EndRoundAsync(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return false;
            }

            // 检查游戏轮次是否可以结束（例如时间是否用完等条件）
            if (game.Status != GameStatus.Playing)
            {
                return false;
            }

            // 可以在这里添加结束游戏轮次的其他逻辑，例如计算本轮得分等
            game.Status = GameStatus.Completed;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 计算游戏得分
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <param name="correctCount">猜对的次数</param>
        /// <returns>计算后的游戏得分</returns>
        public async Task<int> CalculateScoreAsync(int gameId, int correctCount)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                throw new ArgumentException("游戏不存在");
            }

            // 根据猜对次数计算得分，这里可以根据具体的得分规则进行计算
            int score = correctCount * 10; // 示例规则：每次猜对得 10 分

            // 更新游戏得分
            game.RoundScores[game.CurrentRound].Score = score;

            await _context.SaveChangesAsync();
            return score;
        }

        /// <summary>
        /// 获取游戏状态
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <returns>游戏状态对象</returns>
        public async Task<GameStatus> GetGameStatusAsync(int gameId)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                return GameStatus.Waiting;
            }

            return game.Status;
        }

        /// <summary>
        /// 更新游戏状态
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <param name="status">新的游戏状态</param>
        /// <returns>如果成功更新游戏状态返回 true，否则返回 false</returns>
        public async Task<bool> UpdateGameStatusAsync(int gameId, GameStatus status)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return false;
            }

            game.Status = status;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 获取随机词汇
        /// </summary>
        /// <param name="category">词汇类别（可以为空）</param>
        /// <param name="difficulty">词汇难度（可以为空）</param>
        /// <returns>随机词汇对象</returns>
        private async Task<string> GetRandomWordAsync()
        {
            var random = new Random();
            var count = await _context.Words.CountAsync();
            var skip = random.Next(0, count);
            var word = await _context.Words.Skip(skip).FirstOrDefaultAsync();
            return word?.Content ?? "默认词";
        }

        /// <summary>
        /// 处理玩家画图
        /// </summary>
        public async Task<bool> HandleDrawingAsync(int gameId, int playerId, Stroke stroke)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != GameStatus.Playing)
            {
                return false;
            }

            if (game.CurrentDrawingPlayerId != playerId)
            {
                return false;
            }

            var drawingData = new Models.DrawingData
            {
                GameId = gameId,
                PlayerId = playerId,
                Stroke = stroke,
                Timestamp = DateTime.UtcNow
            };

            _context.DrawingData.Add(drawingData);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 处理玩家猜词
        /// </summary>
        public async Task<GuessResult> HandleGuessAsync(int gameId, int playerId, string guessedWord)
        {
            var game = await _context.Games
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != GameStatus.Playing)
            {
                return new GuessResult { Success = false, Message = "游戏不存在或未在进行中" };
            }

            var player = game.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
            {
                return new GuessResult { Success = false, Message = "玩家不存在" };
            }

            if (player.HasGuessed)
            {
                return new GuessResult { Success = false, Message = "你已经猜过了" };
            }

            var isCorrect = guessedWord.ToLower() == game.CurrentWord.ToLower();
            if (isCorrect)
            {
                var score = CalculateScore(game, player);
                player.Score += score;
                player.HasGuessed = true;
                await _context.SaveChangesAsync();

                // 检查是否所有玩家都已猜对
                if (game.Players.All(p => p.HasGuessed))
                {
                    game.Status = GameStatus.Completed;
                    await _context.SaveChangesAsync();
                }
            }

            return new GuessResult
            {
                Success = true,
                IsCorrect = isCorrect,
                Message = isCorrect ? "猜对了！" : "猜错了，继续加油！"
            };
        }

        /// <summary>
        /// 计算得分
        /// </summary>
        private int CalculateScore(Game game, Player player)
        {
            // 基础分数
            const int baseScore = 100;

            // 时间奖励（根据猜测时间计算）
            var timeElapsed = DateTime.UtcNow - game.GameStartedAt.Value;
            var timeBonus = Math.Max(0, 1 - (timeElapsed.TotalSeconds / 60)); // 60秒内完成获得额外分数

            return (int)(baseScore * (1 + timeBonus));
        }

        /// <summary>
        /// 获取当前轮次的画图数据
        /// </summary>
        public async Task<List<Models.DrawingData>> GetCurrentRoundDrawingsAsync(int gameId)
        {
            return await _context.DrawingData
                .Where(d => d.GameId == gameId)
                .OrderBy(d => d.Timestamp)
                .ToListAsync();
        }
        /// <summary>
        /// 根据玩家 ID 获取玩家信息
        /// </summary>
        /// <param name="playerId">玩家 ID</param>
        /// <returns>玩家对象</returns>
        public async Task<Player> GetPlayerByIdAsync(int playerId)
        {
            return await _context.Players.FindAsync(playerId);
        }
        /// <summary>
        /// 创建新玩家
        /// </summary>
        /// <param name="player">玩家对象（需包含 UserId 和 Username）</param>
        /// <returns>创建后的玩家对象（包含数据库生成的 Id）</returns>
        public async Task<Player> CreatePlayerAsync(Player player)
        {
            // 校验必填字段
            if (player.UserId == 0)
            {
                throw new ArgumentException("玩家必须关联 UserId", nameof(player.UserId));
            }

            if (string.IsNullOrEmpty(player.Username))
            {
                player.Username = $"玩家_{player.UserId}"; // 自动生成默认用户名
            }

            // 设置玩家默认状态
            player.Score = 0; // 初始分数为 0
            player.Status = PlayerStatus.Waiting; // 初始状态为等待
            player.LastDrawingTime = null; // 未画过图

            // 添加到数据库
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            Console.WriteLine($"新玩家创建成功：Id={player.Id}, UserId={player.UserId}, 用户名={player.Username}");
            return player;
        }
        /// <summary>
        /// 根据 UserId 查找玩家（关联用户系统的唯一标识）
        /// </summary>
        /// <param name="userId">用户系统中的 UserId</param>
        /// <returns>玩家对象，若不存在则返回 null</returns>
        public async Task<Player> GetPlayerByUserIdAsync(int userId)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

    }

    

    /// <summary>
    /// 画图数据模型
    /// </summary>
    public class DrawingData
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public Stroke Stroke { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 猜词结果模型
    /// </summary>
    public class GuessResult
    {
        public bool Success { get; set; }
        public bool IsCorrect { get; set; }
        public string Message { get; set; }
    }
}