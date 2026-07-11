using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using OnlyBurger.Infrastructure.Features.Products;

namespace OnlyBurger.Api.Controllers;

/// <summary>
/// Menu management. Browsing is open to everyone; creating, updating and deleting
/// products is restricted to administrators.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all menu products.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetProductsQuery(), cancellationToken));

    /// <summary>Gets a single product by id.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetProductByIdQuery(id), cancellationToken));

    /// <summary>Adds a new product to the menu (admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(
            new CreateProductCommand(request.Name, request.Description, request.Price), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Updates an existing product (admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(
            new UpdateProductCommand(id, request.Name, request.Description, request.Price), cancellationToken);
        return Ok(product);
    }

    /// <summary>Deletes a product (admin only).</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
