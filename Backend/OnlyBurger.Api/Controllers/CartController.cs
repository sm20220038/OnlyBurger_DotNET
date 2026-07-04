using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlyBurger.Api.Auth;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Features.Cart;

namespace OnlyBurger.Api.Controllers;

/// <summary>
/// The signed-in user's shopping cart. All actions operate on the caller's own cart,
/// resolved from the JWT — never from a client-supplied user id.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    private readonly ICurrentUser _currentUser;

    public CartController(IDispatcher dispatcher, ICurrentUser currentUser)
    {
        _dispatcher = dispatcher;
        _currentUser = currentUser;
    }

    /// <summary>Returns the current contents of the cart.</summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken cancellationToken)
        => Ok(await _dispatcher.Query(new GetCartQuery(_currentUser.UserId), cancellationToken));

    /// <summary>Adds a product to the cart (increments quantity if already present).</summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> Add(AddToCartRequest request, CancellationToken cancellationToken)
    {
        var cart = await _dispatcher.Send(
            new AddToCartCommand(_currentUser.UserId, request.ProductId, request.Quantity), cancellationToken);
        return Ok(cart);
    }

    /// <summary>Sets the quantity of a product already in the cart.</summary>
    [HttpPut("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> Update(int productId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var cart = await _dispatcher.Send(
            new UpdateCartItemCommand(_currentUser.UserId, productId, request.Quantity), cancellationToken);
        return Ok(cart);
    }

    /// <summary>Removes a product from the cart.</summary>
    [HttpDelete("items/{productId:int}")]
    public async Task<ActionResult<CartDto>> Remove(int productId, CancellationToken cancellationToken)
    {
        var cart = await _dispatcher.Send(
            new RemoveCartItemCommand(_currentUser.UserId, productId), cancellationToken);
        return Ok(cart);
    }
}
