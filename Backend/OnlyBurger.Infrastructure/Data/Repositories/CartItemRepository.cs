using Microsoft.EntityFrameworkCore;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data.Repositories;

/// <summary>EF Core repository for cart line items within a user's active cart.</summary>
public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    public CartItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<CartItem?> GetActiveCartItemAsync(int userId, int productId) =>
        await DbSet.FirstOrDefaultAsync(
            c => c.Cart!.UserId == userId
                 && c.Cart.Status == CartStatus.Active
                 && c.ProductId == productId);

    public async Task<List<CartItem>> GetActiveCartItemsWithProductAsync(int userId) =>
        await DbSet.AsNoTracking()
                   .Include(c => c.Product)
                   .Where(c => c.Cart!.UserId == userId && c.Cart.Status == CartStatus.Active)
                   .OrderBy(c => c.Id)
                   .ToListAsync();
}
