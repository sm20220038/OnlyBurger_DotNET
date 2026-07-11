using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlyBurger.Api.Auth;
using MediatR;
using OnlyBurger.Infrastructure.Features.Orders;

namespace OnlyBurger.Api.Controllers;

/// <summary>
/// Order lifecycle. Customers create and manage their own orders; administrators see all
/// orders and approve/reject them.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public OrdersController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // ----- Customer endpoints -----

    /// <summary>Creates an order from the caller's cart.</summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(
            new CreateOrderCommand(_currentUser.UserId, request.DeliveryLocation), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>Returns the caller's own orders.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetMine(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetUserOrdersQuery(_currentUser.UserId), cancellationToken));
    
    /// <summary>Gets a single order (own order for customers, any order for admins).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(
            new GetOrderByIdQuery(id, _currentUser.UserId, _currentUser.IsAdmin), cancellationToken));

    /// <summary>Modifies a pending order owned by the caller.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderDto>> Update(int id, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(
            new UpdateOrderCommand(_currentUser.UserId, id, request.DeliveryLocation, request.Items), cancellationToken);
        return Ok(order);
    }

    /// <summary>Deletes a pending order owned by the caller.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteOrderCommand(_currentUser.UserId, id), cancellationToken);
        return NoContent();
    }

    /// <summary>Simulates payment for an order owned by the caller.</summary>
    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new PayOrderCommand(_currentUser.UserId, id), cancellationToken));

    // ----- Admin endpoints -----

    /// <summary>Admin: returns every order in the system.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOrdersQuery(), cancellationToken));

    /// <summary>Admin: approves a pending order.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<OrderDto>> Approve(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ApproveOrderCommand(id), cancellationToken));

    /// <summary>Admin: rejects a pending order.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<OrderDto>> Reject(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RejectOrderCommand(id), cancellationToken));

    /// <summary>
    /// Admin: advances the delivery status of an approved order. The change is pushed to the
    /// customer live over SignalR.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/delivery-status")]
    public async Task<ActionResult<OrderDto>> SetDeliveryStatus(
        int id, UpdateDeliveryStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new UpdateDeliveryStatusCommand(id, request.Status), cancellationToken));
}
