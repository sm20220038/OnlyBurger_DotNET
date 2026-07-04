using System.Security.Claims;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Auth;

/// <summary>
/// Reads the current user's identity from the JWT claims on the active HTTP request.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _principal;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _principal = httpContextAccessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;

    public int UserId
    {
        get
        {
            var value = _principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(value, out var id))
            {
                throw new UnauthorizedAppException("The request is not associated with an authenticated user.");
            }

            return id;
        }
    }

    public string Username => _principal?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public UserRole Role =>
        Enum.TryParse<UserRole>(_principal?.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : UserRole.User;

    public bool IsAdmin => Role == UserRole.Admin;
}
