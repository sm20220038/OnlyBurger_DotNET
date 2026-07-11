using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;
using OnlyBurger.Infrastructure.Realtime;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>Admin action: rejects a pending order.</summary>
public record RejectOrderCommand(int OrderId) : IRequest<OrderDto>;

public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderNotifier _notifier;

    public RejectOrderCommandHandler(IUnitOfWork uow, IOrderNotifier notifier)
    {
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<OrderDto> Handle(RejectOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetWithDetailsAsync(command.OrderId)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException($"Only pending orders can be rejected (order is {order.Status}).");
        }

        order.Status = OrderStatus.Rejected;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        var dto = OrderMapper.ToDto(order);
        await _notifier.OrderUpdatedAsync(order.UserId, dto, cancellationToken);
        return dto;
    }
}
