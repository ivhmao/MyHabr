using MyHabr.Entities;

namespace MyHabr.Interfaces
{
    public interface IArticleService
    {
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<Article?> GetArticleByIdAsync(int id);
        void DeleteArticle(Article article);
        void UpdateArticle(Article article);
        void CreateArticle(Article article);

    }
}
