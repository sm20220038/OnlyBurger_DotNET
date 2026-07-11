using OnlyBurger.Infrastructure.Auth;
using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Auth;

/// <summary>Authenticates a user by username/email + password and issues a JWT.</summary>
public record LoginUserCommand(string UsernameOrEmail, string Password) : IRequest<AuthResponse>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(IUnitOfWork uow, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _uow.Users.GetByUsernameOrEmailAsync(command.UsernameOrEmail);

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
