using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Cart;

/// <summary>Returns the current contents of the given user's cart.</summary>
public record GetCartQuery(int UserId) : IRequest<CartDto>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IUnitOfWork _uow;

    public GetCartQueryHandler(IUnitOfWork uow) => _uow = uow;

    public Task<CartDto> Handle(GetCartQuery query, CancellationToken cancellationToken = default)
        => CartBuilder.BuildAsync(_uow, query.UserId);
}
