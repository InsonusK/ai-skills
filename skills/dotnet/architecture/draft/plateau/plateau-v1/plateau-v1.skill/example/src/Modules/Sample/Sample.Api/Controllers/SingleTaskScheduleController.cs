using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Api.Extensions;
using Sample.Interfaces.Commands;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;

namespace Sample.Api.Controllers;

[ApiController]
[Route("api/tasks/{id:int}/schedule")]
public sealed class SingleTaskScheduleController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(TaskScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskScheduleDto>> Get(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskScheduleQuery(id), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return Ok(result.Value);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateTaskScheduleCommand command, CancellationToken ct)
    {
        if (id != command.TaskId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Route/body mismatch",
                Detail = $"Route id {id} does not match command TaskId {command.TaskId}."
            });
        }

        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return Ok();
    }
}
