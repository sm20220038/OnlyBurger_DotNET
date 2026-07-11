using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Cart;

/// <summary>
/// Shared helpers for working with a user's cart. Used by the cart query and by the cart
/// commands so they can all resolve the user's cart and return its up-to-date contents.
/// </summary>
internal static class CartBuilder
{
    /// <summary>
    /// Returns the user's <see cref="CartStatus.Active"/> cart, creating (and saving) an empty
    /// one if they don't have an active cart yet. This is the single entry point for obtaining
    /// the cart the user is currently building.
    /// </summary>
    public static async Task<Domain.Entities.Cart> GetOrCreateCartAsync(IUnitOfWork uow, int userId)
    {
        var cart = await uow.Carts.GetActiveAsync(userId);
        if (cart is null)
        {
            cart = new Domain.Entities.Cart { UserId = userId, Status = CartStatus.Active };
            await uow.Carts.AddAsync(cart);
            await uow.SaveChangesAsync();
        }

        return cart;
    }

    /// <summary>Materializes the given user's active cart into a <see cref="CartDto"/>.</summary>
    public static async Task<CartDto> BuildAsync(IUnitOfWork uow, int userId)
    {
        var lines = await uow.CartItems.GetActiveCartItemsWithProductAsync(userId);

        var items = lines
            .Select(c => new CartItemDto(
                c.Id,
                c.ProductId,
                c.Product!.Name,
                c.Product.Price,
                c.Quantity,
                c.Product.Price * c.Quantity))
            .ToList();

        var total = items.Sum(i => i.LineTotal);
        return new CartDto(items, total);
    }
}
