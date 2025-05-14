namespace backend.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public int GameRoomId { get; set; }
        // public GameRoom GameRoom { get; set; }

        public ChatMessage()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
