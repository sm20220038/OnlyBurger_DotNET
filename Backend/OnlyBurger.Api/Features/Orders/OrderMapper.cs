using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Domain.Entities;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>
/// Maps <see cref="Order"/> entities to <see cref="OrderDto"/> and centralizes the
/// eager-loading of the items + product navigations needed for a complete order view.
/// </summary>
internal static class OrderMapper
{
    public static IQueryable<Order> WithDetails(this IQueryable<Order> query)
        => query.Include(o => o.Items).ThenInclude(i => i.Product);

    public static OrderDto ToDto(Order order) => new(
        order.Id,
        order.UserId,
        order.DeliveryLocation,
        order.OrderDateTime,
        order.TotalPrice,
        order.Status.ToString(),
        order.PaymentStatus.ToString(),
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
