using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Infrastructure.Features.Orders;

/// <summary>
/// Maps <see cref="Order"/> entities to <see cref="OrderDto"/>. The eager-loading of the
/// items + product + customer navigations lives in <c>OrderRepository</c>.
/// </summary>
internal static class OrderMapper
{
    public static OrderDto ToDto(Order order) => new(
        order.Id,
        order.UserId,
        order.User?.PhoneNumber ?? string.Empty,
        order.CartId,
        order.DeliveryLocation,
        order.OrderDateTime,
        order.TotalPrice,
        order.Status.ToString(),
        order.PaymentStatus.ToString(),
        order.DeliveryStatus.ToString(),
        order.Items
            .OrderBy(i => i.Id)
            .Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity))
            .ToList());
}
