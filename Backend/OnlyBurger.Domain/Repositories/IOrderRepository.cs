using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Domain.Repositories;

/// <summary>
/// Repository for orders. Adds the detail-loading reads (items + product + customer) that the
/// order features need, so the eager-loading lives in one place.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>Tracked order with just its line items loaded (for editing).</summary>
    Task<Order?> GetWithItemsAsync(int id);

    /// <summary>Tracked order with items, products and customer loaded (for status changes).</summary>
    Task<Order?> GetWithDetailsAsync(int id);

    /// <summary>Read-only order with items, products and customer loaded.</summary>
    Task<Order?> GetWithDetailsReadOnlyAsync(int id);

    /// <summary>All orders (read-only) with details, newest first.</summary>
    Task<List<Order>> GetAllWithDetailsAsync();

    /// <summary>A single user's orders (read-only) with details, newest first.</summary>
    Task<List<Order>> GetForUserWithDetailsAsync(int userId);
}
