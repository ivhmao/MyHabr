using MyHabr.Enums;

namespace MyHabr.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public int? UpdatedByUserId { get; set; }
        public CommentType CommentType { get; set; }
        public CommentState CommentState { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
        public List<Comment> Children { get; set; } = new();
    }
}
