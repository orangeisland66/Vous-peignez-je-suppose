using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend;

namespace backend.Models
{
    public class Player
    {
        // 玩家唯一标识符
        [Key]
        public int Id { get; set; }

        // 玩家在游戏中的用户名
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        public string Password { get; set; }

        // 玩家所属的游戏房间Id
        public int GameRoomId { get; set; }

        // 玩家所属的房间
        [ForeignKey("GameRoomId")]
        public GameRoom GameRoom { get; set; }

        // 玩家当前的分数
        public int Score { get; set; }

        // 玩家当前状态（例如：待机、作画、猜词等）
        public PlayerStatus Status { get; set; }

        // 玩家是否为房主
        public bool IsHost { get; set; }

        // 玩家是否已经猜对
        public bool HasGuessedCorrectly { get; set; }

        // 玩家是否已作画
        public bool HasDrawn { get; set; }

        // 玩家加入游戏的时间
        public DateTime JoinedAt { get; set; }

        // 玩家退出游戏的时间（如果有的话）
        public DateTime? LeftAt { get; set; }

        public Player()
        {
            Score = 0;
            Status = PlayerStatus.Waiting;  // 默认状态为待机
            HasGuessedCorrectly = false;
            HasDrawn = false;
            JoinedAt = DateTime.UtcNow;
        }
    }

    // 玩家状态枚举
    public enum PlayerStatus
    {
        Waiting = 1,    // 等待中
        Drawing = 2,    // 作画中
        Guessing = 3,   // 猜词中
        Finished = 4    // 游戏结束
    }
}
