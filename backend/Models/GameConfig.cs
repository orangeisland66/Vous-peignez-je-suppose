using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;  // 添加这个
using System.Text.Json;  // 如果需要 JSON 序列化
namespace backend.Models
{
    public class GameConfig
    {
        private int roundTimeLimit {get; set; }
        [NotMapped]
        public Dictionary<string, int> wordDifficultyDistribution { get; set; } = new();
         // 用于数据库存储的 JSON 字符串
        [Column("WordDifficultyDistribution")]
        public string WordDifficultyDistributionJson
        {
            get => JsonSerializer.Serialize(wordDifficultyDistribution);
            set => wordDifficultyDistribution = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(value);
        }
        public GameConfig()
        {
            // Default values for the game configuration
            this.roundTimeLimit = 60; // 60 seconds per round
            this.wordDifficultyDistribution = new Dictionary<string, int>(); // Default distribution
        }
    }
}