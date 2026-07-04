using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>Deletes a pending order owned by the user.</summary>
public record DeleteOrderCommand(int UserId, int OrderId) : ICommand<Unit>;

public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand, Unit>
{
    private readonly AppDbContext _db;

    public DeleteOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<Unit> HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.UserId != command.UserId)
        {
            throw new ForbiddenException("You can only delete your own orders.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException("Only pending orders can be deleted.");
        }

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
