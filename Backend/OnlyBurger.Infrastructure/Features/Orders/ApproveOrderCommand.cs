using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;
using OnlyBurger.Infrastructure.Realtime;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>Admin action: approves a pending order.</summary>
public record ApproveOrderCommand(int OrderId) : IRequest<OrderDto>;

public class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderNotifier _notifier;

    public ApproveOrderCommandHandler(IUnitOfWork uow, IOrderNotifier notifier)
    {
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<OrderDto> Handle(ApproveOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetWithDetailsAsync(command.OrderId)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException($"Only pending orders can be approved (order is {order.Status}).");
        }

        order.Status = OrderStatus.Approved;
        // The order is accepted; the kitchen starts on it right away.
        order.DeliveryStatus = DeliveryStatus.Preparing;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        var dto = OrderMapper.ToDto(order);
        await _notifier.OrderUpdatedAsync(order.UserId, dto, cancellationToken);
        return dto;
    }
}
