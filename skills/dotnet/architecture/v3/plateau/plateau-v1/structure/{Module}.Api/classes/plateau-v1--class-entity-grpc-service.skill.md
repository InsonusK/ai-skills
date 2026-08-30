---
name: plateau-v1--class-entity-grpc-service
description: Class {Entity}GrpcService in the v1 plateau
whenToUse: when implementing the generated gRPC server base class for an entity
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/class
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Goal
- Publish one gRPC method per operation the entity's commands/queries support, each a thin map-dispatch-map adapter

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create.md|{Entity}GrpcService.cs.create]]

# Naming convention
| use case | class name pattern |
| -------- | ------------------- |
| Entity gRPC service | `{Entity}GrpcService` |

# Implementation
```csharp
//Skill: class-entity-grpc-service
//Plateau: v1
//Version: 20260825140000

public sealed class TaskGrpcService(ISender sender) : TaskGrpcService.TaskGrpcServiceBase
{
    public override async Task<TaskReply> GetTask(GetTaskRequest request, ServerCallContext context)
    {
        var result = await sender.Send(new GetTaskByIdQuery(request.Id), context.CancellationToken);
        if (!result.IsSuccess) throw result.ToRpcException();
        return new TaskReply { Id = result.Value.Id, Title = result.Value.Title };
    }

    public override async Task<TaskReply> CreateTask(CreateTaskRequest request, ServerCallContext context)
    {
        var result = await sender.Send(new CreateTaskCommand(request.Title), context.CancellationToken);
        if (!result.IsSuccess) throw result.ToRpcException();
        return new TaskReply { Id = result.Value.Id, Title = result.Value.Title };
    }
}
```

`TaskGrpcServiceBase` is generated from `Task.proto` — never hand-edited. See [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create.md|{Entity}GrpcService.cs.create]].

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create.md|{Entity}GrpcService.cs.create]]

# Rules
MUST:
- Inherit the generated `{Entity}GrpcServiceBase`
- Every RPC method dispatch exactly one `ISender.Send()`, map failure via `ToRpcException()`
MUST NOT:
- Hand-edit generated code

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create.md|{Entity}GrpcService.cs.create]]

# Check list
- [ ] One method per RPC declared in the `.proto`
- [ ] Every method dispatches exactly one `Send()`, maps failure via `ToRpcException()`

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}GrpcService.cs.create.md|{Entity}GrpcService.cs.create]]
