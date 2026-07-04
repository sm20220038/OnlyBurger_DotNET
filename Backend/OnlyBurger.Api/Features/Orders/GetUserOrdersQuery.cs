using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>Returns all orders belonging to a single user, newest first.</summary>
public record GetUserOrdersQuery(int UserId) : IQuery<IReadOnlyList<OrderDto>>;

public class GetUserOrdersQueryHandler : IQueryHandler<GetUserOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly AppDbContext _db;

    public GetUserOrdersQueryHandler(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderDto>> HandleAsync(GetUserOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .WithDetails()
            .Where(o => o.UserId == query.UserId)
            .OrderByDescending(o => o.OrderDateTime)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderMapper.ToDto).ToList();
    }
}
