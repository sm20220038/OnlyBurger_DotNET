using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;
using OnlyBurger.Infrastructure.Realtime;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>
/// Admin action: advances an order's delivery status (Preparing → OutForDelivery → Delivered)
/// and pushes the change to the customer in real time over SignalR.
/// </summary>
public record UpdateDeliveryStatusCommand(int OrderId, DeliveryStatus DeliveryStatus) : IRequest<OrderDto>;

public class UpdateDeliveryStatusCommandHandler : IRequestHandler<UpdateDeliveryStatusCommand, OrderDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderNotifier _notifier;

    public UpdateDeliveryStatusCommandHandler(IUnitOfWork uow, IOrderNotifier notifier)
    {
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<OrderDto> Handle(UpdateDeliveryStatusCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetWithDetailsAsync(command.OrderId)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        // Delivery only makes sense once the order has been accepted.
        if (order.Status != OrderStatus.Approved)
        {
            throw new ValidationException(
                $"Delivery status can only change on approved orders (order is {order.Status}).");
        }

        // Delivery moves forward only — you can't un-deliver an order.
        if (command.DeliveryStatus < order.DeliveryStatus)
        {
            throw new ValidationException(
                $"Delivery status can't move backwards (from {order.DeliveryStatus} to {command.DeliveryStatus}).");
        }

        order.DeliveryStatus = command.DeliveryStatus;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        var dto = OrderMapper.ToDto(order);
        await _notifier.OrderUpdatedAsync(order.UserId, dto, cancellationToken);
        return dto;
    }
}
