using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Domain.Repositories;

/// <summary>Repository for user accounts, used by the authentication features.</summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Finds a user by their username or (normalized) email, or null.</summary>
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

    /// <summary>True if a user already exists with the given username or email.</summary>
    Task<bool> ExistsByUsernameOrEmailAsync(string username, string email);
}
