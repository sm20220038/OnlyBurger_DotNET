using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>
/// Returns a single order. A customer may only view their own orders; an admin may view any.
/// </summary>
public record GetOrderByIdQuery(int OrderId, int RequestingUserId, bool IsAdmin) : IQuery<OrderDto>;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly AppDbContext _db;

    public GetOrderByIdQueryHandler(AppDbContext db) => _db = db;

    public async Task<OrderDto> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .WithDetails()
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {query.OrderId} was not found.");

        if (!query.IsAdmin && order.UserId != query.RequestingUserId)
        {
            throw new ForbiddenException("You can only view your own orders.");
        }

        return OrderMapper.ToDto(order);
    }
}
