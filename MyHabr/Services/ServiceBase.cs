using MyHabr.Helpers;
using MyHabr.Interfaces;
using System.Data.Entity;
using System.Linq.Expressions;

namespace MyHabr.Services
{
    public abstract class ServiceBase<T> : IServiceBase<T> where T : class
    {
        protected AppDbContext DbContext { get; set; }
        public ServiceBase(AppDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public void Create(T entity)
        {
            DbContext.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
            DbContext?.Set<T>().Remove(entity);
        }

        public IQueryable<T> GetAll()
        {
            return DbContext.Set<T>().AsNoTracking();
        }

        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> condition)
        {
            return DbContext.Set<T>().Where(condition).AsNoTracking();
        }

        public void Update(T entity)
        {
            DbContext.Set<T>().Update(entity);
        }
    }
}
