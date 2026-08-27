using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Api.Extensions;
using Sample.Interfaces.Commands;

namespace Sample.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateTaskResult>> Create([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? StatusCodes.Status500InternalServerError, problem);
        }

        // No GET counterpart exists yet — solution-query-integration isn't composed at this plateau,
        // so there is nowhere for CreatedAtAction to point. A write-only module returns 201 with the
        // created resource's body directly; the Location header arrives once a read side exists.
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
