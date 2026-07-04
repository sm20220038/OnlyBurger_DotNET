using System.ComponentModel.DataAnnotations;

namespace OnlyBurger.Api.Features.Orders;

/// <summary>A line within an order, as returned to clients.</summary>
public record OrderItemDto(int Id, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

/// <summary>An order, with its items and computed total, as returned to clients.</summary>
public record OrderDto(
    int Id,
    int UserId,
    string DeliveryLocation,
    DateTime OrderDateTime,
    decimal TotalPrice,
    string Status,
    string PaymentStatus,
    IReadOnlyList<OrderItemDto> Items);

/// <summary>Request body for creating an order from the current cart.</summary>
public record CreateOrderRequest
{
    [Required, MaxLength(300)]
    public string DeliveryLocation { get; init; } = string.Empty;
}

/// <summary>A requested order line when editing a pending order.</summary>
public record OrderLineRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, 1000)]
    public int Quantity { get; init; }
}

/// <summary>
/// Request body for editing a pending order. <see cref="Items"/> is optional: when omitted
/// only the delivery location changes; when supplied it replaces the order's lines.
/// </summary>
public record UpdateOrderRequest
{
    [Required, MaxLength(300)]
    public string DeliveryLocation { get; init; } = string.Empty;

    public List<OrderLineRequest>? Items { get; init; }
}
