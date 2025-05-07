using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
      public class GameRoom
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public RoomStatus status { get; set; }
        public string gameMode { get; set; }
        public bool isPrivate { get; set; }
        public string roomPassword { get; set; }
        public List<Player> players { get; set; }
        public List<ChatMessage> chatHistory { get; set; }
        public GameConfig gameConfig { get; set; }

        public GameRoom()
        {
             // 初始化玩家列表
            players = new List<Player>();
            // 初始化聊天记录列表
            chatHistory = new List<ChatMessage>();
            // 设置默认的房间状态为等待玩家加入
            status = RoomStatus.Waiting;
            // 设置默认的游戏模式（这里假设默认模式为 "五猜一画"）
            gameMode = "五猜一画";
            // 假设默认房间不是私密的
            isPrivate = false;
            // 假设默认房间密码为空字符串
            roomPassword = "";
            // 初始化游戏配置对象（这里可以根据具体需求进一步完善配置项）
            gameConfig = new GameConfig();

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
