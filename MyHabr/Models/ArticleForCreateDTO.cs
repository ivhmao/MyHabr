namespace MyHabr.Models
{
    public class ArticleForCreateDTO
    {
        public string Title { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public IEnumerable<int> Authors { get; set; } = new List<int>();

    }
}
