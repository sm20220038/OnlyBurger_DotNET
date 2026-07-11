using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;
using OnlyBurger.Infrastructure.Realtime;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>
/// Creates an order from the contents of the user's cart. The order date/time and total
/// are set automatically, item prices are captured from the menu, and the cart is checked out.
/// </summary>
public record CreateOrderCommand(int UserId, string DeliveryLocation) : IRequest<OrderDto>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderNotifier _notifier;

    public CreateOrderCommandHandler(IUnitOfWork uow, IOrderNotifier notifier)
    {
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var cart = await _uow.Carts.GetActiveWithItemsAsync(command.UserId);

        if (cart is null || cart.Items.Count == 0)
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
            DeliveryStatus = DeliveryStatus.Pending,
            Items = cart.Items.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.Product!.Price
            }).ToList()
        };
        order.RecalculateTotal();

        // Checkout does NOT throw the cart away: the cart is kept as history, marked
        // checked-out, and linked 1:1 to the order it produced. A fresh active cart is
        // created lazily the next time the user adds something.
        cart.Status = CartStatus.CheckedOut;
        cart.CheckedOutAt = DateTime.UtcNow;
        order.Cart = cart;

        await _uow.Orders.AddAsync(order);
        await _uow.SaveChangesAsync();

        var created = await _uow.Orders.GetWithDetailsReadOnlyAsync(order.Id);
        var dto = OrderMapper.ToDto(created!);

        // Let the admin orders page refresh itself the moment a new order lands.
        await _notifier.AdminOrdersChangedAsync(dto, cancellationToken);
        return dto;
    }
}
