using backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace backend.Models
{
    public class GameHistory
    {

        [Key] // 添加主键属性
        public int Id { get; set; } // 添加主键字段    private User[] players;
        private Dictionary<int, int> scores;
        private Word[] usedWords;
        private DateTime startTime;
        private DateTime endTime;
        private GameStats gameStats;
        public virtual ICollection<PlayerScore> Scores { get; set; } = new List<PlayerScore>();
        public void AddGameStats(GameStats stats)
        {

        }
    }
    public class PlayerScore
    {
        [Key]
        public int Id { get; set; }
        
        public int PlayerId { get; set; }
        public User Player { get; set; }
        
        public int Score { get; set; }
        
        public int GameHistoryId { get; set; }
        // public GameHistory GameHistory { get; set; }
    }
}
