using System;
using System.Collections.Generic;

namespace backend.Models
{
    public class Game
    {
        // 游戏唯一标识符
        public int Id { get; set; }

        // 游戏所属的房间ID
        public int GameRoomId { get; set; }

        // 游戏所属的房间
        public GameRoom GameRoom { get; set; }

        // 当前游戏的状态
        public GameStatus Status { get; set; }

        // 当前轮次
        public int CurrentRound { get; set; }

        // 游戏的最大轮次
        public int MaxRounds { get; set; }

        // 当前作画玩家的ID
        public int CurrentDrawingPlayerId { get; set; }

        // 当前猜词玩家的ID
        public int CurrentGuessingPlayerId { get; set; }

        // 当前轮次的词汇
        public string CurrentWord { get; set; }

        // 游戏的开始时间
        public DateTime? GameStartedAt { get; set; }

        // 游戏历史记录
        public List<GameHistory> GameHistory { get; set; }

        public Player CurrentDrawingPlayer { get; set; }

        public List<Player> Players { get; set; }

        // 每一轮的得分列表
        public List<GameRoundScore> RoundScores { get; set; }

        public Game()
        {
            RoundScores = new List<GameRoundScore>();
            GameHistory = new List<GameHistory>();
            Status = GameStatus.Waiting; // 默认状态为等待
            CurrentRound = 1; // 默认从第1轮开始
            MaxRounds = 5; // 默认最大轮次为5
        }
    }

    // 每一轮的得分实体类
    public class GameRoundScore
    {
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public int Score { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }
    }

    // 游戏状态枚举
    public enum GameStatus
    {
        Waiting = 1,    // 等待玩家准备
        Playing = 2,    // 游戏进行中
        Completed = 3,  // 游戏结束
        Closed = 4      // 游戏被关闭
    }

    // 玩家轮次历史记录
    public class PlayerRoundHistory
    {
        public int PlayerId { get; set; } // 玩家ID
        public int Score { get; set; } // 本轮得分
        public string GuessedWord { get; set; } // 玩家猜的词
        public bool CorrectGuess { get; set; } // 是否猜对
    }
}