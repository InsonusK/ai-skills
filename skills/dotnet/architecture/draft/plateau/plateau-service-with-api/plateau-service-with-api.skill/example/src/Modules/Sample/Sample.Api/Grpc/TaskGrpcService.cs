using Grpc.Core;
using MediatR;
using Sample.Api.Extensions;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

namespace Sample.Api.Grpc;

public sealed class TaskGrpcService(ISender sender) : TaskGrpcService.TaskGrpcServiceBase
{
    public override async Task<TaskReply> CreateTask(CreateTaskRequest request, ServerCallContext context)
    {
        var command = new CreateTaskCommand(request.Title, request.AssigneeId, new SoftEmail(request.AssigneeEmail));
        var result = await sender.Send(command, context.CancellationToken);
        if (!result.IsSuccess)
            throw result.ToRpcException();

        return new TaskReply { Id = result.Value.Id };
    }
}
