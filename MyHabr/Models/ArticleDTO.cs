using MyHabr.Entities;
using MyHabr.Enums;

namespace MyHabr.Models
{
    public class ArticleDTO
    {
        public ArticleDTO(Article article)
        {
            Id = article.Id;
            Title = article.Title;
            CreatedDate = article.CreatedDate;
            Content = article.Content;
            ArticleState = article.ArticleState;
            Authors = article.Authors
                .Select(u=>new UserDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Login = u.Login,
                    Email = u.Email
                });
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Content { get; set; } = String.Empty;
        public ArticleState ArticleState { get; set; }
        public IEnumerable<UserDTO> Authors { get; set; }
    }

}
