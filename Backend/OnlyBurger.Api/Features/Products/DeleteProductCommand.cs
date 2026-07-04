using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Products;

/// <summary>Removes a product from the menu.</summary>
public record DeleteProductCommand(int Id) : ICommand<Unit>;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Unit>
{
    private readonly AppDbContext _db;

    public DeleteProductCommandHandler(AppDbContext db) => _db = db;

    public async Task<Unit> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FindAsync(new object[] { command.Id }, cancellationToken)
            ?? throw new NotFoundException($"Product {command.Id} was not found.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
