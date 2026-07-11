using OnlyBurger.Domain.Entities;

namespace OnlyBurger.Domain.Repositories;

/// <summary>Repository for students.</summary>
public interface IStudentRepository : IRepository<Student>
{
    /// <summary>True if a student with the given index already exists.</summary>
    Task<bool> ExistsByIndexAsync(string index);
}
