using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class GameRoom
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public RoomStatus Status { get; set; }
        public string GameMode { get; set; }
        public bool IsPrivate { get; set; }
        public string RoomPassword { get; set; }
        public List<Player> Players { get; set; }
        public List<ChatMessage> ChatHistory { get; set; }
        public GameConfig GameConfig { get; set; }

        public GameRoom()
        {
            Players = new List<Player>();
            ChatHistory = new List<ChatMessage>();
            Status = RoomStatus.Waiting;
            GameMode = "五猜一画";
            IsPrivate = false;
            RoomPassword = "";
            GameConfig = new GameConfig();
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
