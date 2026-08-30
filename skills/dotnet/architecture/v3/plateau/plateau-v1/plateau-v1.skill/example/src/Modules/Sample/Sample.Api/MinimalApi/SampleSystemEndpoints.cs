using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sample.Api.Extensions;
using Sample.Interfaces.Queries;

namespace Sample.Api.MinimalApi;

public static class SampleSystemEndpoints
{
    public static IEndpointRouteBuilder MapSampleSystemEndpoints(this IEndpointRouteBuilder app)
    {
        // Cross-aggregate read spanning Task + Attachment, backed by App.Queries — no single
        // entity owns this shape, so it does not belong to any of the Controllers above.
        app.MapGet("/api/tasks/{taskId:int}/full", async (int taskId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTaskWithAttachmentsQuery(taskId), ct);
            if (!result.IsSuccess)
            {
                var problem = result.ToProblemDetails();
                return Results.Problem(problem.Detail, statusCode: problem.Status, title: problem.Title);
            }
            return Results.Ok(result.Value);
        });

        return app;
    }
}
