using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>
/// Edits a pending order owned by the user. The delivery location is always updated; when
/// <see cref="Items"/> is provided the order's lines are replaced and the total recomputed.
/// </summary>
public record UpdateOrderCommand(
    int UserId,
    int OrderId,
    string DeliveryLocation,
    IReadOnlyList<OrderLineRequest>? Items) : IRequest<OrderDto>;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateOrderCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<OrderDto> Handle(UpdateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var order = await _uow.Orders.GetWithItemsAsync(command.OrderId)
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

            await ReplaceItemsAsync(order, command.Items);
        }

        await _uow.Orders.UpdateAsync(order);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Orders.GetWithDetailsReadOnlyAsync(order.Id);
        return OrderMapper.ToDto(updated!);
    }

    private async Task ReplaceItemsAsync(Order order, IReadOnlyList<OrderLineRequest> lines)
    {
        // Collapse duplicate product lines into a single line with summed quantity.
        var requested = lines
            .GroupBy(l => l.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        var ids = requested.Keys.ToList();
        var products = (await _uow.Products.FindAsync(p => ids.Contains(p.Id)))
            .ToDictionary(p => p.Id);

        var missing = ids.Where(id => !products.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new NotFoundException($"Product(s) not found: {string.Join(", ", missing)}.");
        }

        // Clearing the tracked collection makes EF delete the removed lines (cascade),
        // then the new lines are inserted on save.
        order.Items.Clear();
        foreach (var kvp in requested)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = kvp.Key,
                Quantity = kvp.Value,
                UnitPrice = products[kvp.Key].Price
            });
        }

        order.RecalculateTotal();
    }
}
