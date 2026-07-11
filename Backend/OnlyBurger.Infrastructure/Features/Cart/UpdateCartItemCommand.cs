using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Cart;

/// <summary>Sets the quantity of an existing cart line belonging to the user.</summary>
public record UpdateCartItemCommand(int UserId, int ProductId, int Quantity) : IRequest<CartDto>;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateCartItemCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CartDto> Handle(UpdateCartItemCommand command, CancellationToken cancellationToken = default)
    {
        var item = await _uow.CartItems.GetActiveCartItemAsync(command.UserId, command.ProductId)
            ?? throw new NotFoundException($"Product {command.ProductId} is not in the cart.");

        item.Quantity = command.Quantity;

        await _uow.CartItems.UpdateAsync(item);
        await _uow.SaveChangesAsync();
        return await CartBuilder.BuildAsync(_uow, command.UserId);
    }
}
