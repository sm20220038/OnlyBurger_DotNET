using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Entities;

namespace OnlyBurger.Api.Features.Cart;

/// <summary>
/// Adds a product to the user's cart. If the product is already in the cart the quantity
/// is increased rather than creating a duplicate line.
/// </summary>
public record AddToCartCommand(int UserId, int ProductId, int Quantity) : ICommand<CartDto>;

public class AddToCartCommandHandler : ICommandHandler<AddToCartCommand, CartDto>
{
    private readonly AppDbContext _db;

    public AddToCartCommandHandler(AppDbContext db) => _db = db;

    public async Task<CartDto> HandleAsync(AddToCartCommand command, CancellationToken cancellationToken = default)
    {
        var productExists = await _db.Products.AnyAsync(p => p.Id == command.ProductId, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException($"Product {command.ProductId} was not found.");
        }

        var cart = await CartBuilder.GetOrCreateCartAsync(_db, command.UserId, cancellationToken);

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == command.ProductId, cancellationToken);

        if (existing is null)
        {
            _db.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = command.ProductId,
                Quantity = command.Quantity
            });
        }
        else
        {
            existing.Quantity += command.Quantity;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await CartBuilder.BuildAsync(_db, command.UserId, cancellationToken);
    }
}
