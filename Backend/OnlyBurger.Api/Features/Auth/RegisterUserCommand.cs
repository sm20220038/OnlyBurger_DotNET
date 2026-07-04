using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Auth;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;
using OnlyBurger.Api.Domain.Entities;
using OnlyBurger.Api.Domain.Enums;

namespace OnlyBurger.Api.Features.Auth;

/// <summary>Registers a new customer (always created with the <see cref="UserRole.User"/> role).</summary>
public record RegisterUserCommand(string Username, string Email, string Password) : ICommand<AuthResponse>;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserCommandHandler(AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var username = command.Username.Trim();
        var email = command.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(
            u => u.Username == username || u.Email == email, cancellationToken);
        if (exists)
        {
            throw new ConflictException("A user with that username or email already exists.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.Hash(command.Password),
            Role = UserRole.User,
            // Every user owns exactly one cart; create it up front.
            Cart = new Domain.Entities.Cart()
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.CreateToken(user);
        return new AuthResponse(user.Id, user.Username, user.Email, user.Role.ToString(), token.Token, token.ExpiresAtUtc);
    }
}
