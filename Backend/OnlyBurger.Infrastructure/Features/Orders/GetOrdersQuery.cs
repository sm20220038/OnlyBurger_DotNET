using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>Admin query: returns every order in the system, newest first.</summary>
public record GetOrdersQuery() : IRequest<IReadOnlyList<OrderDto>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IUnitOfWork _uow;

    public GetOrdersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken = default)
    {
        var orders = await _uow.Orders.GetAllWithDetailsAsync();
        return orders.Select(OrderMapper.ToDto).ToList();
    }
}
