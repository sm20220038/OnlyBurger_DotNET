using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Entities;

namespace OnlyBurger.Api.Features.Products;

/// <summary>Adds a new product to the menu.</summary>
public record CreateProductCommand(string Name, string Description, decimal Price) : ICommand<ProductDto>;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductDto>
{
    private readonly AppDbContext _db;

    public CreateProductCommandHandler(AppDbContext db) => _db = db;

    public async Task<ProductDto> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = command.Name.Trim(),
            Description = command.Description.Trim(),
            Price = command.Price
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
