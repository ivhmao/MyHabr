using MyHabr.Enums;

namespace MyHabr.Entities
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; }
        public ArticleState ArticleState { get; set; }
        public IEnumerable<User> Authors { get; set; }
    }
}
