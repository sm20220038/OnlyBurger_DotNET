using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Products;

/// <summary>Updates an existing product's details.</summary>
public record UpdateProductCommand(int Id, string Name, string Description, decimal Price) : ICommand<ProductDto>;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, ProductDto>
{
    private readonly AppDbContext _db;

    public UpdateProductCommandHandler(AppDbContext db) => _db = db;

    public async Task<ProductDto> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FindAsync(new object[] { command.Id }, cancellationToken)
            ?? throw new NotFoundException($"Product {command.Id} was not found.");

        product.Name = command.Name.Trim();
        product.Description = command.Description.Trim();
        product.Price = command.Price;

        await _db.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
