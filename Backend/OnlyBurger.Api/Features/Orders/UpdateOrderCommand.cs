using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Entities;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>
/// Edits a pending order owned by the user. The delivery location is always updated; when
/// <see cref="Items"/> is provided the order's lines are replaced and the total recomputed.
/// </summary>
public record UpdateOrderCommand(
    int UserId,
    int OrderId,
    string DeliveryLocation,
    IReadOnlyList<OrderLineRequest>? Items) : ICommand<OrderDto>;

public class UpdateOrderCommandHandler : ICommandHandler<UpdateOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;

    public UpdateOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<OrderDto> HandleAsync(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {command.OrderId} was not found.");

        if (order.UserId != command.UserId)
        {
            throw new ForbiddenException("You can only modify your own orders.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new ValidationException("Only pending orders can be modified.");
        }

        order.DeliveryLocation = command.DeliveryLocation.Trim();

        if (command.Items is not null)
        {
            if (command.Items.Count == 0)
            {
                throw new ValidationException("An order must contain at least one item.");
            }

            await ReplaceItemsAsync(order, command.Items, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var updated = await _db.Orders.AsNoTracking().WithDetails()
            .FirstAsync(o => o.Id == order.Id, cancellationToken);
        return OrderMapper.ToDto(updated);
    }

    private async Task ReplaceItemsAsync(Order order, IReadOnlyList<OrderLineRequest> lines, CancellationToken cancellationToken)
    {
        // Collapse duplicate product lines into a single line with summed quantity.
        var requested = lines
            .GroupBy(l => l.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        var products = await _db.Products
            .Where(p => requested.Keys.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var missing = requested.Keys.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException($"Product(s) not found: {string.Join(", ", missing)}.");
        }

        _db.OrderItems.RemoveRange(order.Items);
        order.Items = requested
            .Select(kvp => new OrderItem
            {
                ProductId = kvp.Key,
                Quantity = kvp.Value,
                UnitPrice = products[kvp.Key].Price
            })
            .ToList();

        order.RecalculateTotal();
    }
}
