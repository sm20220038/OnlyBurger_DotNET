using OnlyBurger.Infrastructure.Auth;
using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Enums;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Auth;

/// <summary>Registers a new customer (always created with the <see cref="UserRole.User"/> role).</summary>
public record RegisterUserCommand(string Username, string Email, string PhoneNumber, string Password) : IRequest<AuthResponse>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUserCommandHandler(IUnitOfWork uow, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var username = command.Username.Trim();
        var email = command.Email.Trim().ToLowerInvariant();

        if (await _uow.Users.ExistsByUsernameOrEmailAsync(username, email))
        {
            throw new ConflictException("A user with that username or email already exists.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PhoneNumber = command.PhoneNumber.Trim(),
            PasswordHash = _passwordHasher.Hash(command.Password),
            Role = UserRole.User,
            // Start the user off with an empty active cart.
            Carts = new List<Domain.Entities.Cart> { new() { Status = CartStatus.Active } }
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        var token = _jwtTokenService.CreateToken(user);
        return new AuthResponse(user.Id, user.Username, user.Email, user.Role.ToString(), token.Token, token.ExpiresAtUtc);
    }
}
