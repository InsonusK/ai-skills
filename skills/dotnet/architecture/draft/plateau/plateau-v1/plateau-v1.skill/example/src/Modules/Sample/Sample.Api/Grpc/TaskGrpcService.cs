using Grpc.Core;
using MediatR;
using Sample.Api.Extensions;
using Sample.Api.Grpc;
using Sample.Interfaces.Commands;
using Sample.Interfaces.Queries;
using Sample.Interfaces.ValueObjects;

namespace Sample.Api.Grpc.Services;

public sealed class TaskGrpcService(ISender sender) : global::Sample.Api.Grpc.TaskGrpcService.TaskGrpcServiceBase
{
    public override async Task<TaskReply> GetTask(GetTaskRequest request, ServerCallContext context)
    {
        var result = await sender.Send(new GetTaskSummaryQuery(request.Id), context.CancellationToken);
        if (!result.IsSuccess) throw result.ToRpcException();
        return new TaskReply { Id = result.Value.Id, Title = result.Value.Title };
    }

    public override async Task<TaskReply> CreateTask(CreateTaskRequest request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateTaskCommand(
                new SoftTitle(request.Title),
                request.AssigneeId,
                new SoftEmail(request.AssigneeEmail),
                DateTimeOffset.TryParse(request.ActionTimestamp, out var actionTimestamp) ? actionTimestamp : DateTimeOffset.MinValue,
                ParseOptional(request.StartDateTime),
                ParseOptional(request.DueDateTime)),
            context.CancellationToken);

        if (!result.IsSuccess) throw result.ToRpcException();
        return new TaskReply { Id = result.Value.Id, Title = request.Title };
    }

    private static DateTimeOffset? ParseOptional(string? value)
        => string.IsNullOrWhiteSpace(value) || !DateTimeOffset.TryParse(value, out var parsed) ? null : parsed;
}
