using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Infrastructure.Auth;

/// <summary>
/// Result of issuing a JWT: the encoded token and the moment it expires.
/// </summary>
public record TokenResult(string Token, DateTime ExpiresAtUtc);

/// <summary>
/// Issues signed JWT access tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    TokenResult CreateToken(User user);
}
