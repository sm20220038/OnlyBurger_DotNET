using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Products;

/// <summary>Returns all products on the menu.</summary>
public record GetProductsQuery() : IQuery<IReadOnlyList<ProductDto>>;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly AppDbContext _db;

    public GetProductsQueryHandler(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProductDto>> HandleAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price))
            .ToListAsync(cancellationToken);
    }
}
