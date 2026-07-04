namespace OnlyBurger.Api.Auth;

/// <summary>
/// Hashes and verifies user passwords. Plain-text passwords are never stored.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
