// backend/Models/ActiveGameState.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For NotMapped if you use it
using System.Collections.Generic; // For List if you use it for WordChoices

namespace backend.Models
{
    public class ActiveGameState
    {
        [Key]
        public int Id { get; set; }

        // 外键: 关联到 GameRoom 的主键 (GameRoom.Id)
        public int GameRoomId { get; set; }
        public virtual GameRoom GameRoom { get; set; } // 导航属性

        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; } // 这个值在游戏开始时根据 GameRoom.Rounds 或玩家数设定

        public int? CurrentPainterUserId { get; set; } // 当前画师的 User.Id, 可空
        //public virtual User? CurrentPainter { get; set; } // (可选) 导航到画师User对象, 需要配置外键 CurrentPainterUserId

        public string? CurrentTargetWord { get; set; } // 画师选中的词

        public GamePhase CurrentGamePhase { get; set; }

        // 存储提供给画师的词语选项 (通常是临时的，不在数据库持久化)
        // 如果需要持久化，可以考虑序列化为JSON字符串或单独的表
        [NotMapped] // EF Core 将不会为这个属性创建数据库列
        public List<string> WordChoicesForPainter { get; set; } = new List<string>();

        // 存储玩家得分, 例如: {"userId1": 10, "userId2": 20}
        // 使用 string 存储 JSON 序列化后的得分数据
        public string PlayerScoresJson { get; set; }

        // 回合开始时间戳 (可选, 如果需要精确计时或追溯)
        //打开了，用于处理时钟同步
        public DateTime? RoundStartTime { get; set; }
        public ActiveGameState()
        {
            CurrentRound = 0; // 通常在游戏正式开始第一轮时设为 1
            TotalRounds = 0;
            CurrentGamePhase = GamePhase.NotStarted;
            PlayerScoresJson = "{}"; // 初始化为空的JSON对象字符串
            WordChoicesForPainter = new List<string>();
        }
    }
        public enum GamePhase
    {
        NotStarted = 0,                         // 游戏尚未开始（或 ActiveGameState 刚创建）
        WaitingForPainterToChooseWord = 1,      // 等待当前画师选择词语
        PainterChoosingWord = 2,             // (可选) 如果需要一个状态明确画师正在选，而其他人等待
        DrawingAndGuessing = 3,                 // 画师作画，其他玩家猜词阶段
        RoundOver = 4,                          // 当前回合结束（展示结果，准备下一轮或结束游戏）
        GameOver = 5                            // 整场游戏结束
    }
}
