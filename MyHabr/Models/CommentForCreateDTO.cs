namespace MyHabr.Models
{
    public class CommentForCreateDTO
    {
        public int ArticleId { get; set; }
        public string Content { get; set; } = String.Empty;
        public int ParentId { get; set; }
    }
}
