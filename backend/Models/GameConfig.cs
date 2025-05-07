namespace backend.Models
{
    public class GameConfig
    {
        private int roundTimeLimit {get; set; }
         public Dictionary<string, int> wordDifficultyDistribution{ get; set; }
        public GameConfig()
        {
            // Default values for the game configuration
            this.roundTimeLimit = 60; // 60 seconds per round
            this.wordDifficultyDistribution = new Dictionary<string, int>(); // Default distribution
        }
    }
}