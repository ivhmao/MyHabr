namespace MyHabr.Interfaces
{
    public interface IServiceWrapper
    {
        IArticleService Article { get; }
        IUserService User { get; }
        ICommentService Comment { get; }

        Task SaveAsync();
    }
}