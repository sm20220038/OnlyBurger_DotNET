using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Entities;

namespace OnlyBurger.Api.Features.Cart;

/// <summary>
/// Shared helpers for working with a user's cart. Used by the cart query and by the cart
/// commands so they can all resolve the user's cart and return its up-to-date contents.
/// </summary>
internal static class CartBuilder
{
    /// <summary>
    /// Returns the user's cart, creating (and saving) an empty one if they don't have it yet.
    /// A user owns exactly one cart, so this is the single entry point for obtaining it.
    /// </summary>
    public static async Task<Domain.Entities.Cart> GetOrCreateCartAsync(
        AppDbContext db, int userId, CancellationToken cancellationToken)
    {
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (cart is null)
        {
            cart = new Domain.Entities.Cart { UserId = userId };
            db.Carts.Add(cart);
            await db.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }

    /// <summary>Materializes the given user's cart into a <see cref="CartDto"/>.</summary>
    public static async Task<CartDto> BuildAsync(AppDbContext db, int userId, CancellationToken cancellationToken)
    {
        var items = await db.CartItems
            .AsNoTracking()
            .Where(c => c.Cart!.UserId == userId)
            .OrderBy(c => c.Id)
            .Select(c => new CartItemDto(
                c.Id,
                c.ProductId,
                c.Product!.Name,
                c.Product.Price,
                c.Quantity,
                c.Product.Price * c.Quantity))
            .ToListAsync(cancellationToken);

        var total = items.Sum(i => i.LineTotal);
        return new CartDto(items, total);
    }
}
