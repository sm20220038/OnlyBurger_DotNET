using System.ComponentModel.DataAnnotations;

namespace OnlyBurger.Infrastructure.Features.Students;

public record StudentDto(string Index, string Ime, string Prezime);

public record CreateStudentRequest
{
    [Required, MaxLength(9), MinLength(9)]
    public string Index { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Ime { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Prezime { get; init; } = string.Empty;
}
