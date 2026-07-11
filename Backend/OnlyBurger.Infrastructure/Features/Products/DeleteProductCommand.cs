using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Products;

/// <summary>Removes a product from the menu.</summary>
public record DeleteProductCommand(int Id) : IRequest<Unit>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public DeleteProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _uow.Products.GetByIdAsync(command.Id)
            ?? throw new NotFoundException($"Product {command.Id} was not found.");

        await _uow.Products.RemoveAsync(product);
        await _uow.SaveChangesAsync();

        return Unit.Value;
    }
}
