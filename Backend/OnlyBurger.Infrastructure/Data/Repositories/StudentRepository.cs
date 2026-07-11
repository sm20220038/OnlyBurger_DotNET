using Microsoft.EntityFrameworkCore;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Data.Repositories;

/// <summary>EF Core repository for students.</summary>
public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByIndexAsync(string index) =>
        await DbSet.AnyAsync(s => s.Index == index);
}
