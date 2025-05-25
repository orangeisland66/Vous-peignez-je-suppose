using System;
using System.ComponentModel.DataAnnotations;
namespace backend.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        
        public int SenderId { get; set; }
        public User Sender { get; set; }
        
        public string GameRoomId { get; set; }  // 改为 string 类型
        public GameRoom GameRoom { get; set; }

        public ChatMessage()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
