using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;
using OnlyBurger.Infrastructure.Realtime;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>Deletes a pending order owned by the user.</summary>
public record DeleteOrderCommand(int UserId, int OrderId) : IRequest<Unit>;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderNotifier _notifier;

    public DeleteOrderCommandHandler(IUnitOfWork uow, IOrderNotifier notifier)
    {
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<Unit> Handle(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetByIdAsync(command.OrderId)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.UserId != command.UserId)
        {
            throw new ForbiddenException("You can only delete your own orders.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException("Only pending orders can be deleted.");
        }

        var dto = OrderMapper.ToDto(order);
        await _uow.Orders.RemoveAsync(order);
        await _uow.SaveChangesAsync();

        // Tell the admin orders page to reload so the cancelled order drops off the list.
        await _notifier.AdminOrdersChangedAsync(dto, cancellationToken);

        return Unit.Value;
    }
}
