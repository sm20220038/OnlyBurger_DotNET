using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of the generic <see cref="IRepository{T}"/>. Entity-specific
/// repositories derive from this and add their own query methods.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await DbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.Where(predicate).ToListAsync();

    public async Task<T?> GetByIdAsync(params object[] keyValues) =>
        await DbSet.FindAsync(keyValues);

    public async Task AddAsync(T entity) =>
        await DbSet.AddAsync(entity);

    public Task RemoveAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
}
