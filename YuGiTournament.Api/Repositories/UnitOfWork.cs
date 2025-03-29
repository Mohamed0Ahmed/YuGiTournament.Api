using YuGiTournament.Api.Abstractions;
using YuGiTournament.Api.Data;

namespace YuGiTournament.Api.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = [];

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepository<T>(this);
                _repositories[type] = repository;
            }
            return (IGenericRepository<T>)_repositories[type];
        }

        public ApplicationDbContext GetDbContext()
        {
            return _context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}