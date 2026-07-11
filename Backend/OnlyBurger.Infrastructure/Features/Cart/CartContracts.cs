using System.ComponentModel.DataAnnotations;

namespace OnlyBurger.Infrastructure.Features.Cart;

/// <summary>A single line in the cart, with the product's current price and line total.</summary>
public record CartItemDto(int Id, int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

/// <summary>The full cart for a user, plus the running total.</summary>
public record CartDto(IReadOnlyList<CartItemDto> Items, decimal TotalPrice);

/// <summary>Request body for adding a product to the cart.</summary>
public record AddToCartRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; init; }

    [Range(1, 1000)]
    public int Quantity { get; init; } = 1;
}

/// <summary>Request body for setting the quantity of a cart line.</summary>
public record UpdateCartItemRequest
{
    [Range(1, 1000)]
    public int Quantity { get; init; }
}
