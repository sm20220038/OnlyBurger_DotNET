using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>Admin action: rejects a pending order.</summary>
public record RejectOrderCommand(int OrderId) : ICommand<OrderDto>;

public class RejectOrderCommandHandler : ICommandHandler<RejectOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;

    public RejectOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<OrderDto> HandleAsync(RejectOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.WithDetails()
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException($"Only pending orders can be rejected (order is {order.Status}).");
        }

        order.Status = OrderStatus.Rejected;
        await _db.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
