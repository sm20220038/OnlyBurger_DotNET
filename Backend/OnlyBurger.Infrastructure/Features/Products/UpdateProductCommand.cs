using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Products;

/// <summary>Updates an existing product's details.</summary>
public record UpdateProductCommand(int Id, string Name, string Description, decimal Price) : IRequest<ProductDto>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProductDto> Handle(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _uow.Products.GetByIdAsync(command.Id)
            ?? throw new NotFoundException($"Product {command.Id} was not found.");

        product.Name = command.Name.Trim();
        product.Description = command.Description.Trim();
        product.Price = command.Price;

        await _uow.Products.UpdateAsync(product);
        await _uow.SaveChangesAsync();

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
