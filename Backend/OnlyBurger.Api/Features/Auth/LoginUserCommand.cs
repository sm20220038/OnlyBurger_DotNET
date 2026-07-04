using Microsoft.EntityFrameworkCore;
using OnlyBurger.Api.Auth;
using OnlyBurger.Api.Common.Exceptions;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

namespace OnlyBurger.Api.Features.Auth;

/// <summary>Authenticates a user by username/email + password and issues a JWT.</summary>
public record LoginUserCommand(string UsernameOrEmail, string Password) : ICommand<AuthResponse>;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, AuthResponse>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var identifier = command.UsernameOrEmail.Trim();
        var normalizedEmail = identifier.ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Username == identifier || u.Email == normalizedEmail, cancellationToken);

        // Same error whether the user is missing or the password is wrong, to avoid
        // revealing which accounts exist.
        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Invalid username/email or password.");
        }

        var token = _jwtTokenService.CreateToken(user);
        return new AuthResponse(user.Id, user.Username, user.Email, user.Role.ToString(), token.Token, token.ExpiresAtUtc);
    }
}
