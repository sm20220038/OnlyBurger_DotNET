using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Cart;

/// <summary>
/// Adds a product to the user's cart. If the product is already in the cart the quantity
/// is increased rather than creating a duplicate line.
/// </summary>
public record AddToCartCommand(int UserId, int ProductId, int Quantity) : IRequest<CartDto>;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly IUnitOfWork _uow;

    public AddToCartCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CartDto> Handle(AddToCartCommand command, CancellationToken cancellationToken = default)
    {
        if (await _uow.Products.GetByIdAsync(command.ProductId) is null)
        {
            throw new NotFoundException($"Product {command.ProductId} was not found.");
        }

        var cart = await CartBuilder.GetOrCreateCartAsync(_uow, command.UserId);

        var existing = await _uow.CartItems.GetActiveCartItemAsync(command.UserId, command.ProductId);

        if (existing is null)
        {
            await _uow.CartItems.AddAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = command.ProductId,
                Quantity = command.Quantity
            });
        }
        else
        {
            existing.Quantity += command.Quantity;
            await _uow.CartItems.UpdateAsync(existing);
        }

        await _uow.SaveChangesAsync();
        return await CartBuilder.BuildAsync(_uow, command.UserId);
    }
}
