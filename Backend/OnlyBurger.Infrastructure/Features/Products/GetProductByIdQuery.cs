using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Products;

/// <summary>Returns a single product by id.</summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _uow;

    public GetProductByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        var product = await _uow.Products.GetByIdAsync(query.Id)
            ?? throw new NotFoundException($"Product {query.Id} was not found.");

        return new ProductDto(product.Id, product.Name, product.Description, product.Price);
    }
}
