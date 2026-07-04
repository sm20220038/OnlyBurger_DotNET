using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>Admin action: approves a pending order.</summary>
public record ApproveOrderCommand(int OrderId) : ICommand<OrderDto>;

public class ApproveOrderCommandHandler : ICommandHandler<ApproveOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;

    public ApproveOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<OrderDto> HandleAsync(ApproveOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.WithDetails()
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException($"Only pending orders can be approved (order is {order.Status}).");
        }

        order.Status = OrderStatus.Approved;
        await _db.SaveChangesAsync(cancellationToken);

        return OrderMapper.ToDto(order);
    }
}
