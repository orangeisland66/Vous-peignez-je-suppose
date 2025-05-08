namespace backend.Models
{
    public class Stroke
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Point[] Coordinates { get; set; }
        public int BrushSize { get; set; }
        public string Color { get; set; }
    }
}

