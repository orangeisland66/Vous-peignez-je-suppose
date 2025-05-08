namespace backend.Models
{
    public class GameStats
    {
        public int Id { get; set; }
        public int AverageGuessTime { get; set; }
        public float CorrectRate { get; set; }
        public List<Word> PopularWords { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int TotalPlayers { get; set; }
        public DateTime LastUpdated { get; set; }

        public GameStats()
        {
            PopularWords = new List<Word>();
            LastUpdated = DateTime.UtcNow;
        }
    }
}