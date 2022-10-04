using MyHabr.Entities;
using MyHabr.Enums;

namespace MyHabr.Models
{
    public class CommentForUpdateDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = String.Empty;
    }
}
