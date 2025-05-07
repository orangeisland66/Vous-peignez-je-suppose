using backend.Models;
namespace backend.Models
{
    public class GameHistory
{
    private User[] players;
    private Dictionary<int, int> scores;
    private Word[] usedWords;
    private DateTime startTime;
    private DateTime endTime;
    private GameStats gameStats;
    public void AddGameStats(GameStats stats)
    {

    }
}
}