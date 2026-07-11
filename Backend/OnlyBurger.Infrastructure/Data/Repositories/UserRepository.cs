using Microsoft.EntityFrameworkCore;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data.Repositories;

/// <summary>EF Core repository for user accounts.</summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        var identifier = usernameOrEmail.Trim();
        var normalizedEmail = identifier.ToLowerInvariant();
        return await DbSet.FirstOrDefaultAsync(
            u => u.Username == identifier || u.Email == normalizedEmail);
    }

    public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email) =>
        await DbSet.AnyAsync(u => u.Username == username || u.Email == email);
}
