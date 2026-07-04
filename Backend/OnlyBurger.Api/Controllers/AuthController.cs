using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Features.Auth;

namespace OnlyBurger.Api.Controllers;

/// <summary>Registration and login. Both endpoints are public.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AuthController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>Registers a new customer account and returns a JWT.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.Send(
            new RegisterUserCommand(request.Username, request.Email, request.Password), cancellationToken);
        return Ok(result);
    }

    /// <summary>Logs in with username/email + password and returns a JWT.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.Send(
            new LoginUserCommand(request.UsernameOrEmail, request.Password), cancellationToken);
        return Ok(result);
    }
}
