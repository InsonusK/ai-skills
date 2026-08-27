using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Api.Extensions;
using Sample.Interfaces.Commands;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;

namespace Sample.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:int}/attachments")]
public sealed class TaskAttachmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AttachmentSummaryDto>>> List(int taskId, CancellationToken ct)
    {
        var result = await sender.Send(new ListTaskAttachmentsQuery(taskId), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateAttachmentResult), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateAttachmentResult>> Add(int taskId, [FromBody] CreateAttachmentCommand command, CancellationToken ct)
    {
        if (taskId != command.TaskId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Route/body mismatch",
                Detail = $"Route taskId {taskId} does not match command TaskId {command.TaskId}."
            });
        }

        var result = await sender.Send(command, ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return CreatedAtAction(
            nameof(SingleTaskAttachmentController.Get), "SingleTaskAttachment",
            new { taskId, attachmentGuid = command.Guid }, result.Value);
    }
}
