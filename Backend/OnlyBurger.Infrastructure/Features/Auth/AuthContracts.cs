using System.ComponentModel.DataAnnotations;

namespace OnlyBurger.Infrastructure.Features.Auth;

/// <summary>Request body for registering a new customer account.</summary>
public record RegisterRequest
{
    [Required, MaxLength(100)]
    public string Username { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required, Phone, MaxLength(30)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Request body for logging in. Accepts either the username or the email.</summary>
public record LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Successful authentication result, including the JWT to use on later requests.</summary>
public record AuthResponse(
    int UserId,
    string Username,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAtUtc);
