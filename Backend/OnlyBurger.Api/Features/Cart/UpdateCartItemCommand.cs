using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Cart;

/// <summary>Sets the quantity of an existing cart line belonging to the user.</summary>
public record UpdateCartItemCommand(int UserId, int ProductId, int Quantity) : ICommand<CartDto>;

public class UpdateCartItemCommandHandler : ICommandHandler<UpdateCartItemCommand, CartDto>
{
    private readonly AppDbContext _db;

    public UpdateCartItemCommandHandler(AppDbContext db) => _db = db;

    public async Task<CartDto> HandleAsync(UpdateCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(c => c.Cart!.UserId == command.UserId && c.ProductId == command.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product {command.ProductId} is not in the cart.");

        item.Quantity = command.Quantity;

        await _db.SaveChangesAsync(cancellationToken);
        return await CartBuilder.BuildAsync(_db, command.UserId, cancellationToken);
    }
}
