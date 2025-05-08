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
                .Include(g => g.GameRoom)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != GameStatus.Waiting)
            {
                return false;
            }

            // 获取随机词汇
            var word = await _wordManager.GetRandomWordAsync();
            if (word == null)
            {
                return false;
            }

            // 更新游戏状态
            game.CurrentWord = word.Content;
            game.Status = GameStatus.Playing;
            game.GameStartedAt = DateTime.UtcNow;

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
            game.RoundScores[game.CurrentRound] = score;

            await _context.SaveChangesAsync();
            return score;
        }

        /// <summary>
        /// 获取游戏状态
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <returns>游戏状态对象</returns>
        public async Task<Game> GetGameStatusAsync(int gameId)
        {
            return await _context.Games
                .Include(g => g.GameRoom)
                .ThenInclude(gr => gr.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);
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
        public async Task<Word> GetRandomWordAsync(string category = null, string difficulty = null)
        {
            IQueryable<Word> query = _context.Words;

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(w => w.Category == category);
            }

            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(w => w.Difficulty == difficulty);
            }

            return await query.OrderBy(w => Guid.NewGuid()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 处理玩家画图
        /// </summary>
        public async Task<bool> HandleDrawingAsync(int gameId, int playerId, Stroke stroke)
        {
            var game = await _context.Games
                .Include(g => g.GameRoom)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != GameStatus.Playing)
            {
                return false;
            }

            // 检查是否是当前作画玩家
            if (game.CurrentDrawingPlayerId != playerId)
            {
                return false;
            }

            // 保存画图数据
            var drawingData = new DrawingData
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
                .Include(g => g.GameRoom)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || game.Status != GameStatus.Playing)
            {
                return new GuessResult { Success = false, Message = "游戏状态不正确" };
            }

            // 检查是否是当前作画玩家
            if (game.CurrentDrawingPlayerId == playerId)
            {
                return new GuessResult { Success = false, Message = "作画玩家不能猜词" };
            }

            // 检查是否已经猜对
            var player = await _context.Players.FindAsync(playerId);
            if (player.HasGuessedCorrectly)
            {
                return new GuessResult { Success = false, Message = "已经猜对了" };
            }

            // 检查猜词是否正确
            bool isCorrect = string.Equals(guessedWord, game.CurrentWord, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                // 更新玩家状态
                player.HasGuessedCorrectly = true;
                player.Score += CalculateScore(game);

                // 检查是否所有玩家都已猜对
                var allPlayersGuessed = await CheckAllPlayersGuessedAsync(gameId);
                if (allPlayersGuessed)
                {
                    game.Status = GameStatus.Completed;
                }

                await _context.SaveChangesAsync();
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
        private int CalculateScore(Game game)
        {
            // 基础分数
            int baseScore = 100;

            // 根据游戏时间计算时间加成
            var timeElapsed = DateTime.UtcNow - game.GameStartedAt.Value;
            double timeBonus = Math.Max(0, 1 - (timeElapsed.TotalSeconds / 60)); // 每分钟减少1%的分数

            // 计算最终得分
            return (int)(baseScore * (1 + timeBonus));
        }

        /// <summary>
        /// 检查是否所有玩家都已猜对
        /// </summary>
        private async Task<bool> CheckAllPlayersGuessedAsync(int gameId)
        {
            var game = await _context.Games
                .Include(g => g.GameRoom)
                .ThenInclude(gr => gr.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                return false;
            }

            // 排除作画玩家
            var guessingPlayers = game.GameRoom.Players
                .Where(p => p.Id != game.CurrentDrawingPlayerId)
                .ToList();

            // 检查是否所有猜词玩家都已猜对
            return guessingPlayers.All(p => p.HasGuessedCorrectly);
        }

        /// <summary>
        /// 获取当前轮次的画图数据
        /// </summary>
        public async Task<List<DrawingData>> GetCurrentRoundDrawingsAsync(int gameId)
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