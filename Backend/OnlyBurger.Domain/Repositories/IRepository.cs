using System.Linq.Expressions;

namespace OnlyBurger.Domain.Repositories;

/// <summary>
/// Generic repository over an aggregate. Provides the common data-access operations so the
/// application layer (the CQRS handlers) never talks to EF Core / the DbContext directly.
/// Entity-specific queries live on the derived interfaces (e.g. <see cref="IOrderRepository"/>).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(params object[] keyValues);
    Task AddAsync(T entity);
    Task RemoveAsync(T entity);
    Task UpdateAsync(T entity);
}
