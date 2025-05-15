using System.ComponentModel.DataAnnotations;
namespace backend.Models
{
    public class Point
    {
        [Key]
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}