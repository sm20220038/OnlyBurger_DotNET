using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>
/// Returns a single order. A customer may only view their own orders; an admin may view any.
/// </summary>
public record GetOrderByIdQuery(int OrderId, int RequestingUserId, bool IsAdmin) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IUnitOfWork _uow;

    public GetOrderByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<OrderDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetWithDetailsReadOnlyAsync(query.OrderId)
            ?? throw new NotFoundException($"Order {query.OrderId} was not found.");

        if (!query.IsAdmin && order.UserId != query.RequestingUserId)
        {
            throw new ForbiddenException("You can only view your own orders.");
        }

        return OrderMapper.ToDto(order);
    }
}
