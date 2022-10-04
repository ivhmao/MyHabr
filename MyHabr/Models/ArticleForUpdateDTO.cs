using MyHabr.Enums;

namespace MyHabr.Models
{
    public class ArticleForUpdateDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public IEnumerable<int> Authors { get; set; } = new List<int>();
    }
}
