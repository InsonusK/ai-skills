---
description: The wire contract for one entity's gRPC service — the single source of truth the C# server base class is generated from
project_name: "{Module}.Api"
name: "{Entity}.proto"
element_kind: proto
change_kind: create
tags:
  - solution/grpc-integration
  - element/entity-proto
---

# Goals
- Declare the RPC methods one entity actually needs and the request/response message shapes for each, as the single source of truth `Grpc.Tools` generates the C# server base class from

# Core Principles
- One `.proto` file, one `service`, per entity — never one solution-wide `.proto` file with every entity's RPCs in it
- Message fields are numbered once and never renumbered — protobuf wire compatibility depends on field numbers, not names
- Only declare the RPC methods the module's commands/queries actually support — a `.proto` service with an RPC nothing implements is exactly the "dead capability" `solution-cecil-architecture-tests`' dead-rule check exists to catch, just in a different layer

# Naming convention
| use case | file name pattern | file name | service name | package |
| -------- | ------------------ | ---------- | ------------- | ------- |
| Entity gRPC contract | `{Entity}.proto` | `Task.proto` | `{Entity}GrpcService` | `{module}` (lowercase) |

# Implementation changes

```protobuf
// {Module}.Api/Protos/Task.proto
syntax = "proto3";

option csharp_namespace = "TaskModule.Api.Grpc";

package task;

service TaskGrpcService {
  rpc GetTask (GetTaskRequest) returns (TaskReply);
  rpc CreateTask (CreateTaskRequest) returns (TaskReply);
}

message GetTaskRequest {
  int32 id = 1;
}

message CreateTaskRequest {
  string title = 1;
}

message TaskReply {
  int32 id = 1;
  string title = 2;
}
```

Only `GetTask`/`CreateTask` are declared here because that's what this module's commands/queries currently support — a module with only `solution-mediator-integration` composed (no query-integration yet) would declare `CreateTask` alone, with `GetTask` added once a query exists to back it.

# Rule changes

## MUST
- One `service` per `.proto` file, named `{Entity}GrpcService`
- Declare only RPC methods backed by a real command/query
- Message field numbers assigned once, never reused or renumbered after the first release
- Never declare an RPC method with no command/query behind it
- Never put more than one entity's service in one `.proto` file

# Check list
- [ ] One `.proto` file per entity, one `service` inside it
- [ ] Every declared RPC method has a real command/query to dispatch
- [ ] Message field numbers are stable and never reused
