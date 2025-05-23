using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;

namespace backend.Repositories
{
    public class WordRepository
    {
        private readonly OurDbContext _context;

        public WordRepository(OurDbContext context)
        {
            _context = context;
        }

        // 获取所有词汇
        public IEnumerable<Word> GetAllWords()
        {
            return _context.Words.ToList();
        }

        // 根据 ID 获取单个词汇
        public Word GetWordById(int wordId)
        {
            return _context.Words.FirstOrDefault(w => w.Id == wordId);
        }

        // 根据类别和难度获取词汇列表
        public IEnumerable<Word> GetWordsByCategoryAndDifficulty(string category, string difficulty)
        {
            return _context.Words.Where(w => w.Category == category).ToList();
        }

        // 添加新词汇
        public void AddWord(Word word)
        {
            _context.Words.Add(word);
            _context.SaveChanges();
        }

        // 更新词汇
        public void UpdateWord(Word word)
        {
            _context.Entry(word).State = EntityState.Modified;
            _context.SaveChanges();
        }

        // 删除词汇
        public void DeleteWord(int wordId)
        {
            var word = _context.Words.FirstOrDefault(w => w.Id == wordId);
            if (word != null)
            {
                _context.Words.Remove(word);
                _context.SaveChanges();
            }
        }
    }
}