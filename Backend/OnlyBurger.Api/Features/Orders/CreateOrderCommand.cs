using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Entities;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>
/// Creates an order from the contents of the user's cart. The order date/time and total
/// are set automatically, item prices are captured from the menu, and the cart is emptied.
/// </summary>
public record CreateOrderCommand(int UserId, string DeliveryLocation) : ICommand<OrderDto>;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly AppDbContext _db;

    public CreateOrderCommandHandler(AppDbContext db) => _db = db;

    public async Task<OrderDto> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.Cart!.UserId == command.UserId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
        {
            throw new ValidationException("Your cart is empty. Add products before creating an order.");
        }

        var order = new Order
        {
            UserId = command.UserId,
            DeliveryLocation = command.DeliveryLocation.Trim(),
            OrderDateTime = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid,
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Product!.Price
            }).ToList()
        };
        order.RecalculateTotal();

        _db.Orders.Add(order);
        // Checkout clears the cart.
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await _db.Orders.AsNoTracking().WithDetails()
            .FirstAsync(o => o.Id == order.Id, cancellationToken);
        return OrderMapper.ToDto(created);
    }
}
