using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Products;

/// <summary>Returns all products on the menu.</summary>
public record GetProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IUnitOfWork _uow;

    public GetProductsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken = default)
    {
        var products = await _uow.Products.GetAllAsync();
        return products
            .OrderBy(p => p.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price))
            .ToList();
    }
}
