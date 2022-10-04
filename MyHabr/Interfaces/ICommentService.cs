using MyHabr.Entities;

namespace MyHabr.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<Comment?> GetCommentByIdAsync(int id);
        void DeleteComment(Comment comment);
        void UpdateComment(Comment comment);
        void CreateComment(Comment comment);
        Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(int id);
        Task<IEnumerable<Comment>> GetCommentByIdAsync(IEnumerable<int> listOfIds);
    }
}
