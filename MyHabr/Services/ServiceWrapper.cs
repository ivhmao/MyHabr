using MyHabr.Helpers;
using MyHabr.Interfaces;

namespace MyHabr.Services
{
    public class ServiceWrapper : IServiceWrapper
    {
        private AppDbContext _appDbContext;
        private IUserService _userService;
        private IArticleService _articleService;
        private ICommentService _commentService;

        public IUserService User { get { return _userService; } }
        public IArticleService Article { get { return _articleService; } }
        public ICommentService Comment { get { return _commentService; } }

        public ServiceWrapper(AppDbContext appDbContext, IUserService userService, IArticleService articleService, ICommentService commentService)
        {
            _appDbContext = appDbContext;
            _userService = userService;
            _articleService = articleService;
            _commentService = commentService;
        }

        public async Task SaveAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}
