using MediatR;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Products;

/// <summary>Adds a new product to the menu.</summary>
public record CreateProductCommand(string Name, string Description, decimal Price) : IRequest<ProductDto>;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _uow;

    public CreateProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = command.Name.Trim(),
            Description = command.Description.Trim(),
            Price = command.Price
        };

        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
