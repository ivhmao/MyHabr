using Microsoft.EntityFrameworkCore;
using MyHabr.Entities;
using MyHabr.Helpers;
using MyHabr.Interfaces;

namespace MyHabr.Services
{
    public class ArticleService : ServiceBase<Article>, IArticleService
    {

        public ArticleService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public void CreateArticle(Article article)
        {
            Create(article);
        }

        public void DeleteArticle(Article article)
        {
            Delete(article);
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await GetAll()
                .Include(a=>a.Authors)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await GetByCondition(a => a.Id == id)
                .Include(a => a.Authors)
                .FirstOrDefaultAsync();
        }

        public void UpdateArticle(Article article)
        {
            Update(article);
        }
    }
}
