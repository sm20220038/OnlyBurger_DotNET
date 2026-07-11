using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Domain.Repositories;

/// <summary>Repository for cart line items within a user's active cart.</summary>
public interface ICartItemRepository : IRepository<CartItem>
{
    /// <summary>A single line for the given product in the user's active cart, or null.</summary>
    Task<CartItem?> GetActiveCartItemAsync(int userId, int productId);

    /// <summary>All lines in the user's active cart, with their products, ordered.</summary>
    Task<List<CartItem>> GetActiveCartItemsWithProductAsync(int userId);
}
