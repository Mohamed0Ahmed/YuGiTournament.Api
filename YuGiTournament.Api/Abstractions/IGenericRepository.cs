using System.Linq.Expressions;

namespace YuGiTournament.Api.Abstractions
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T?> GetByIdAsync(string id);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities); 
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities); 
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}