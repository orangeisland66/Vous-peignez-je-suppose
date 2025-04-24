using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace you_draw_i_guess_project.Models
{
    public class GameRoom
    {
        // 房间唯一标识符
        public int Id { get; set; }

        // 房间名称，最大长度100
        [Required]
        [StringLength(100)]
        public string RoomName { get; set; }

        // 房间的创建时间
        public DateTime CreatedAt { get; set; }

        // 房间当前的状态
        public RoomStatus Status { get; set; }

        // 游戏的开始时间
        public DateTime? GameStartedAt { get; set; }

        // 当前轮次
        public int CurrentRound { get; set; }

        // 房间内的玩家列表
        public List<Player> Players { get; set; }

        // 当前游戏的词汇（可选，基于实际需求，可能是从词库中获取）
        public string CurrentWord { get; set; }

        // 房间的最大玩家人数
        public int MaxPlayers { get; set; }

        // 游戏的总轮次
        public int MaxRounds { get; set; }

        public GameRoom()
        {
            Players = new List<Player>();
            CreatedAt = DateTime.UtcNow;
            Status = RoomStatus.Waiting;  // 默认房间状态为等待
            CurrentRound = 1;  // 默认从第1轮开始
            MaxPlayers = 10;  // 默认最大玩家人数为10
            MaxRounds = 5;  // 默认最大轮次数为5
        }
    }

    // 房间状态枚举
    public enum RoomStatus
    {
        Waiting = 1,    // 等待玩家加入
        Playing = 2,    // 游戏进行中
        Completed = 3,  // 游戏结束
        Closed = 4      // 房间关闭
    }
}
