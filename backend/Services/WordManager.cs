using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;

namespace backend.Services
{
    public class WordManager
    {
        private readonly OurDbContext _context;

        public WordManager(OurDbContext context)
        {
            _context = context;
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


            return await query.OrderBy(w => Guid.NewGuid()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 添加词汇
        /// </summary>
        /// <param name="word">要添加的词汇对象</param>
        public async Task AddWordAsync(Word word)
        {
            _context.Words.Add(word);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 更新词汇
        /// </summary>
        /// <param name="word">要更新的词汇对象</param>
        public async Task UpdateWordAsync(Word word)
        {
            _context.Entry(word).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 创建多个词汇
        /// </summary>
        /// <param name="wordDataList">包含词汇内容、类别和难度的列表</param>
        /// <returns>创建的词汇列表</returns>
        public List<Word> CreateMultipleWords(List<(string content, string category, string difficulty)> wordDataList)
        {
            var words = new List<Word>();
            foreach (var (content, category, difficulty) in wordDataList)
            {
                var word = new Word
                {
                    Content = content,
                    Category = category,
                };
                words.Add(word);
            }
            return words;
        }
        // 假设这是在某个服务或控制器中的方法

    }
}