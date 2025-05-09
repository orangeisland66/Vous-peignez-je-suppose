using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;

namespace backend.Services
{
    public class GameService
    {
        private readonly OurDbContext _context;
        private readonly WordManager _wordManager;

        public GameService(OurDbContext context, WordManager wordManager)
        {
            _context = context;
            _wordManager = wordManager;
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