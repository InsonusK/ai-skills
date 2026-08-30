using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Api.Extensions;
using Sample.Interfaces.Commands;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;

namespace Sample.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateTaskResult>> Create([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return CreatedAtAction(nameof(SingleTaskController.Get), "SingleTask", new { id = result.Value.Id }, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskSummaryDto>>> List(CancellationToken ct)
    {
        var result = await sender.Send(new ListTasksQuery(), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return Ok(result.Value);
    }
}
