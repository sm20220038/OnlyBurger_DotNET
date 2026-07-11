using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>
/// Simulates payment for the user's own order (no real payment gateway). Flips the order's
/// payment status to Paid.
/// </summary>
public record PayOrderCommand(int UserId, int OrderId) : IRequest<OrderDto>;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _uow;

    public PayOrderCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<OrderDto> Handle(PayOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetWithDetailsAsync(command.OrderId)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.UserId != command.UserId)
        {
            throw new ForbiddenException("You can only pay for your own orders.");
        }

        if (order.Status == OrderStatus.Rejected)
        {
            throw new ValidationException("A rejected order cannot be paid.");
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            throw new ValidationException("This order has already been paid.");
        }

        order.PaymentStatus = PaymentStatus.Paid;
        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        return OrderMapper.ToDto(order);
    }
}
