using System;

namespace backend.Models
{
    public class DrawingData
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public Stroke Stroke { get; set; }
        public DateTime Timestamp { get; set; }
    }
}