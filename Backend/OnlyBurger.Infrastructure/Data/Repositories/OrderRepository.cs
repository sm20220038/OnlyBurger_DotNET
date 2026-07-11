using Microsoft.EntityFrameworkCore;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data.Repositories;

/// <summary>EF Core repository for orders, with the detail-loading reads in one place.</summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    // Items + their products + the customer — everything an order view/notification needs.
    private static IQueryable<Order> WithDetails(IQueryable<Order> query) =>
        query.Include(o => o.User)
             .Include(o => o.Items)
             .ThenInclude(i => i.Product);

    public async Task<Order?> GetWithItemsAsync(int id) =>
        await DbSet.Include(o => o.Items)
                   .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetWithDetailsAsync(int id) =>
        await WithDetails(DbSet).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetWithDetailsReadOnlyAsync(int id) =>
        await WithDetails(DbSet.AsNoTracking()).FirstOrDefaultAsync(o => o.Id == id);

    public async Task<List<Order>> GetAllWithDetailsAsync() =>
        await WithDetails(DbSet.AsNoTracking())
              .OrderByDescending(o => o.OrderDateTime)
              .ToListAsync();

    public async Task<List<Order>> GetForUserWithDetailsAsync(int userId) =>
        await WithDetails(DbSet.AsNoTracking())
              .Where(o => o.UserId == userId)
              .OrderByDescending(o => o.OrderDateTime)
              .ToListAsync();
}
