// E:\m_Documents\Project\Vous-peignez-je-suppose\backend\Models\GameRoom.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class GameRoom
    {
        // **数据库主键，通常由数据库自动生成**
        // 前端发送的 roomId (string) 不会绑定到这个 Id (int)
        [Key]
        public int Id { get; set; }

        // **添加 RoomId 属性以匹配前端发送的数据**
        // 考虑是否需要唯一索引或后端生成此ID
        public string? RoomId { get; set; } // 八位随机字符串，由前端创建房间时产生

        [Required(ErrorMessage = "房间名称不能为空")] // 添加Required验证
        public string Name { get; set; } // 将Name改为非可空，并添加Required验证

        public RoomStatus Status { get; set; }

        public string GameMode { get; set; }

        public bool IsPrivate { get; set; }

        // 密码，私密房间需要
        // 考虑是否需要加密存储密码，以及是否需要RequiredIf验证
        public string? RoomPassword { get; set; } // 使用可空字符串匹配前端发送 null 的情况

        [Required(ErrorMessage = "最大玩家数不能为空")] // 添加Required验证
        [Range(2, 12, ErrorMessage = "最大玩家数必须在 {1} 到 {2} 之间")] // 添加Range验证
        public int MaxPlayers { get; set; }

        // **添加 Rounds 属性以匹配前端发送的数据**
        [Required(ErrorMessage = "游戏回合数不能为空")] // 添加Required验证
        [Range(4, 10, ErrorMessage = "游戏回合数必须在 {1} 到 {2} 之间")] // 假设回合数在 4, 6, 8, 10 之间
        public int Rounds { get; set; }

        // **添加 Categories 属性以匹配前端发送的数据**
        [Required(ErrorMessage = "请至少选择一个词库分类")] // 添加Required验证
        [MinLength(1, ErrorMessage = "请至少选择一个词库分类")] // 至少选择一个分类
        public List<string> Categories { get; set; }

        // 新增: 导航到对应的活动游戏状态 (如果游戏正在进行)
        // 一个 GameRoom 在某一时刻最多只有一个 ActiveGameState
        public virtual ActiveGameState? ActiveState { get; set; } // 可空，因为房间可能不在游戏状态

            // **新增：创建者用户ID**
        public int CreatorId { get; set; }
        // **新增：导航属性到创建者用户 **
        // [ForeignKey("CreatorId")] // 如果 User 模型的主键不是 Id，或者想明确指定外键
        public virtual User? Creator { get; set; } // 创建者

        public List<Player> Players { get; set; } // 这个属性通常在创建时为空列表
        public List<ChatMessage> ChatHistory { get; set; } // 这个属性通常在创建时为空列表
        public GameConfig GameConfig { get; set; } // 这个属性可能需要根据前端数据进行映射

        public GameRoom()
        {
            // 构造函数中的默认值仍然有用，但模型绑定会覆盖它们
            Players = new List<Player>();
            ChatHistory = new List<ChatMessage>();
            Status = RoomStatus.Waiting;
            GameMode = "五猜一画"; // 默认游戏模式
            IsPrivate = false;
            RoomPassword = ""; // 默认空密码
            MaxPlayers = 6; // 默认最大玩家数
            Rounds = 6; // 默认回合数
            Categories = new List<string>(); // 默认空分类列表
            GameConfig = new GameConfig(); // 默认游戏配置
            //CreatorId在创建时被赋值
        }
    }

    // 房间状态枚举 (保持不变)
    public enum RoomStatus
    {
        Waiting = 0,    // 等待玩家加入
        Playing = 1,    // 游戏进行中
        Completed = 2,  // 游戏结束
        Closed = 3      // 房间关闭
    }
}
