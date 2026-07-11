using OnlyBurger.Domain.Enums;

namespace OnlyBurger.Api.Auth;

/// <summary>
/// Exposes the identity of the caller behind the current request. Handlers use this to
/// scope queries to the logged-in user and to enforce ownership rules.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    string Username { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
}
