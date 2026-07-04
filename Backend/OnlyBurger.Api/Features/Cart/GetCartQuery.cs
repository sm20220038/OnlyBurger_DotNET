using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Cart;

/// <summary>Returns the current contents of the given user's cart.</summary>
public record GetCartQuery(int UserId) : IQuery<CartDto>;

public class GetCartQueryHandler : IQueryHandler<GetCartQuery, CartDto>
{
    private readonly AppDbContext _db;

    public GetCartQueryHandler(AppDbContext db) => _db = db;

    public Task<CartDto> HandleAsync(GetCartQuery query, CancellationToken cancellationToken = default)
        => CartBuilder.BuildAsync(_db, query.UserId, cancellationToken);
}
