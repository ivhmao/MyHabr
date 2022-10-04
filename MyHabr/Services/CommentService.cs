using MyHabr.Entities;
using MyHabr.Helpers;
using MyHabr.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MyHabr.Services
{
    public class CommentService : ServiceBase<Comment>, ICommentService
    {
        public CommentService(AppDbContext dbContext) : base(dbContext)
        {
        }

        public void CreateComment(Comment comment)
        {
            Create(comment);
        }

        public void DeleteComment(Comment comment)
        {
            Delete(comment);
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await GetAll()
                .OrderBy(x => x.Created)
                .ToListAsync();
        }
        public async Task<IEnumerable<Comment>> GetCommentByIdAsync(IEnumerable<int> listOfIds)
        {
            return await GetByCondition(x=> listOfIds.Contains(x.Id))
                .OrderBy(x => x.Created)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await GetByCondition(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(int id)
        {
            return await GetByCondition(x => x.ArticleId == id)
                .OrderBy(x => x.Created)
                .ToListAsync();
        }

        public void UpdateComment(Comment comment)
        {
            Update(comment);
        }
    }
}
