using Microsoft.EntityFrameworkCore;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data.Repositories;

/// <summary>EF Core repository for carts, scoped to a user's single active cart.</summary>
public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetActiveAsync(int userId) =>
        await DbSet.FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

    public async Task<Cart?> GetActiveWithItemsAsync(int userId) =>
        await DbSet.Include(c => c.Items)
                   .ThenInclude(i => i.Product)
                   .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);
}
