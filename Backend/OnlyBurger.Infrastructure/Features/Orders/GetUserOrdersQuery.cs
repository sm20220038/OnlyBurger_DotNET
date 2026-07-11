using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>Returns all orders belonging to a single user, newest first.</summary>
public record GetUserOrdersQuery(int UserId) : IRequest<IReadOnlyList<OrderDto>>;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IUnitOfWork _uow;

    public GetUserOrdersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<OrderDto>> Handle(GetUserOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _uow.Orders.GetForUserWithDetailsAsync(query.UserId);
        return orders.Select(OrderMapper.ToDto).ToList();
    }
}
