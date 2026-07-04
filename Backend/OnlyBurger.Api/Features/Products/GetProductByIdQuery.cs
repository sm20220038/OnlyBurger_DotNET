using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Products;

/// <summary>Returns a single product by id.</summary>
public record GetProductByIdQuery(int Id) : IQuery<ProductDto>;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    private readonly AppDbContext _db;

    public GetProductByIdQueryHandler(AppDbContext db) => _db = db;

    public async Task<ProductDto> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product {query.Id} was not found.");

        return product;
    }
}
