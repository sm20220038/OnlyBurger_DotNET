using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Cart;

/// <summary>Removes a product from the user's cart.</summary>
public record RemoveCartItemCommand(int UserId, int ProductId) : ICommand<CartDto>;

public class RemoveCartItemCommandHandler : ICommandHandler<RemoveCartItemCommand, CartDto>
{
    private readonly AppDbContext _db;

    public RemoveCartItemCommandHandler(AppDbContext db) => _db = db;

    public async Task<CartDto> HandleAsync(RemoveCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Cart!.UserId == command.UserId && c.ProductId == command.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product {command.ProductId} is not in the cart.");

        _db.CartItems.Remove(item);

        await _db.SaveChangesAsync(cancellationToken);
        return await CartBuilder.BuildAsync(_db, command.UserId, cancellationToken);
    }
}
