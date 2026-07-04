using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>Admin query: returns every order in the system, newest first.</summary>
public record GetOrdersQuery() : IQuery<IReadOnlyList<OrderDto>>;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly AppDbContext _db;

    public GetOrdersQueryHandler(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrderDto>> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .WithDetails()
            .OrderByDescending(o => o.OrderDateTime)
            .ToListAsync(cancellationToken);

        return orders.Select(OrderMapper.ToDto).ToList();
    }
}
