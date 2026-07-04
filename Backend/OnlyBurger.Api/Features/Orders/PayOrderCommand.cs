using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>
/// Simulates payment for the user's own order (no real payment gateway). Flips the order's
/// payment status to Paid.
/// </summary>
public record PayOrderCommand(int UserId, int OrderId) : ICommand<OrderDto>;

public class PayOrderCommandHandler : ICommandHandler<PayOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;

    public PayOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<OrderDto> HandleAsync(PayOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.WithDetails()
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken)
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
        await _db.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
