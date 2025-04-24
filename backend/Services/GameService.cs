using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Services
{
    public class GameService
    {
        private readonly DbContext _context;

        public GameService(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 开始游戏轮次
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <returns>如果成功开始游戏轮次返回 true，否则返回 false</returns>
        public async Task<bool> StartRoundAsync(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return false;
            }

            // 检查游戏是否可以开始新的一轮（例如当前轮次是否已结束等条件）
            if (game.currentRound >= game.maxRounds)
            {
                return false;
            }

            // 可以在这里添加开始游戏轮次的其他逻辑，例如重置本轮的相关数据等
            game.currentRound++;
            game.remainingTime = game.gameConfig.roundTimeLimit;
            game.gameStatus = "playing";

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
            if (game.remainingTime > 0)
            {
                return false;
            }

            // 可以在这里添加结束游戏轮次的其他逻辑，例如计算本轮得分等
            game.gameStatus = "round_ended";

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
            game.drawerScore += score;

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
             .Include(g => g.gameConfig)
             .FirstOrDefaultAsync(g => g.id == gameId);
        }

        /// <summary>
        /// 更新游戏状态
        /// </summary>
        /// <param name="gameId">游戏 ID</param>
        /// <param name="status">新的游戏状态</param>
        /// <returns>如果成功更新游戏状态返回 true，否则返回 false</returns>
        public async Task<bool> UpdateGameStatusAsync(int gameId, string status)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return false;
            }

            game.gameStatus = status;

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
                query = query.Where(w => w.category == category);
            }

            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(w => w.difficulty == difficulty);
            }

            return await query.OrderBy(w => Guid.NewGuid()).FirstOrDefaultAsync();
        }
    }
}