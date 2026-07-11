using Microsoft.AspNetCore.Mvc;
using MediatR;
using OnlyBurger.Infrastructure.Features.Students;

namespace OnlyBurger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("kreiraj-studenta")]
    public async Task<ActionResult<StudentDto>> Create(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await _mediator.Send(
            new CreateStudentCommand(request.Index, request.Ime, request.Prezime), cancellationToken);
        return Ok(student);
    }
}
