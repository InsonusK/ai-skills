using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.Api.Extensions;
using Sample.Interfaces.Commands;
using Sample.Interfaces.DTOs;
using Sample.Interfaces.Queries;

namespace Sample.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:int}/attachments/{attachmentGuid:guid}")]
public sealed class SingleTaskAttachmentController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AttachmentSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentSummaryDto>> Get(int taskId, System.Guid attachmentGuid, CancellationToken ct)
    {
        var result = await sender.Send(new GetTaskAttachmentQuery(taskId, attachmentGuid), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return Ok(result.Value);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int taskId, System.Guid attachmentGuid, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteAttachmentCommand(taskId, attachmentGuid), ct);
        if (!result.IsSuccess)
        {
            var problem = result.ToProblemDetails();
            return StatusCode(problem.Status ?? 500, problem);
        }
        return NoContent();
    }
}
