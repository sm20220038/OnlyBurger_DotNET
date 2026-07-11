namespace OnlyBurger.Domain.Repositories;

/// <summary>
/// Unit of Work: exposes every repository and commits all of their changes in a single
/// transaction via <see cref="SaveChangesAsync"/>. All repositories share one DbContext, so a
/// handler can touch several of them and persist everything atomically with one save.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICartRepository Carts { get; }
    ICartItemRepository CartItems { get; }
    IUserRepository Users { get; }
    IStudentRepository Students { get; }

    /// <summary>Persists all pending changes across the repositories. Returns rows affected.</summary>
    Task<int> SaveChangesAsync();
}
