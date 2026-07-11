using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Cart;

/// <summary>Removes a product from the user's cart.</summary>
public record RemoveCartItemCommand(int UserId, int ProductId) : IRequest<CartDto>;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartDto>
{
    private readonly IUnitOfWork _uow;

    public RemoveCartItemCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CartDto> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var item = await _uow.CartItems.GetActiveCartItemAsync(command.UserId, command.ProductId)
            ?? throw new NotFoundException($"Product {command.ProductId} is not in the cart.");

        await _uow.CartItems.RemoveAsync(item);

        await _uow.SaveChangesAsync();
        return await CartBuilder.BuildAsync(_uow, command.UserId);
    }
}
