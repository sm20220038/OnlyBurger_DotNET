using OnlyBurger.Infrastructure.Common.Exceptions;
using MediatR;
using OnlyBurger.Domain.Entities;
using OnlyBurger.Domain.Repositories;

namespace OnlyBurger.Infrastructure.Features.Students;


public record CreateStudentCommand(string Index, string Ime, string Prezime) : IRequest<StudentDto>;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IUnitOfWork _uow;

    public CreateStudentCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<StudentDto> Handle(CreateStudentCommand command, CancellationToken cancellationToken = default)
    {
        var index = command.Index.Trim();

        if (await _uow.Students.ExistsByIndexAsync(index))
            throw new ConflictException($"Student sa indeksom: {index} vec postoji!");

        var student = new Student
        {
            Index = index,
            Ime = command.Ime.Trim(),
            Prezime = command.Prezime.Trim()
        };

        await _uow.Students.AddAsync(student);
        await _uow.SaveChangesAsync();

        return new StudentDto(student.Index, student.Ime, student.Prezime);
    }
}
