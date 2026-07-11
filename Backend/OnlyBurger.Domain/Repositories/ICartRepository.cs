using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Domain.Repositories;

/// <summary>Repository for shopping carts, scoped to a user's single active cart.</summary>
public interface ICartRepository : IRepository<Cart>
{
    /// <summary>The user's active cart (no items loaded), or null if they don't have one.</summary>
    Task<Cart?> GetActiveAsync(int userId);

    /// <summary>The user's active cart with its items and their products loaded.</summary>
    Task<Cart?> GetActiveWithItemsAsync(int userId);
}
