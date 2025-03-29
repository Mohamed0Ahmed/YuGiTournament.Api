using YuGiTournament.Api.Data;

namespace YuGiTournament.Api.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        ApplicationDbContext GetDbContext();
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}