---
name: class-entity-proto
description: The {Entity}.proto wire contract in the v1 plateau
whenToUse: when declaring or extending the gRPC contract for one entity
domain: skill
type: template
plateau: v1
version: 20260825140000
tags:
  - skill/template/proto
  - plateau/v1
created_by:
  - "[[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]]"
---

# Goal
- Declare the RPC methods one entity actually needs, as the single source of truth the C# server base class is generated from

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create.md|{Entity}.proto.create]]

# Naming convention
| use case | file name pattern | service name |
| -------- | ------------------ | ------------- |
| Entity gRPC contract | `{Entity}.proto` | `{Entity}GrpcService` |

# Implementation
```protobuf
// Skill: class-entity-proto
// Plateau: v1
// Version: 20260825140000

syntax = "proto3";
option csharp_namespace = "TaskModule.Api.Grpc";
package task;

service TaskGrpcService {
  rpc GetTask (GetTaskRequest) returns (TaskReply);
  rpc CreateTask (CreateTaskRequest) returns (TaskReply);
}

message GetTaskRequest { int32 id = 1; }
message CreateTaskRequest { string title = 1; }
message TaskReply { int32 id = 1; string title = 2; }
```

Only RPCs backed by a real command/query are declared — see [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create.md|{Entity}.proto.create]].

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create.md|{Entity}.proto.create]]

# Rules
MUST:
- One `service` per `.proto` file
- Message field numbers stable, never reused after first release
MUST NOT:
- Declare an RPC with no command/query behind it

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create.md|{Entity}.proto.create]]

# Check list
- [ ] One `.proto` per entity, one `service` inside
- [ ] Every declared RPC has a real command/query

__Applied solutions:__
- [[../../../../../solutions/solution-grpc-integration.skill/solution-grpc-integration.skill.md|solution-grpc-integration]] - [[../../../../../solutions/solution-grpc-integration.skill/Implementation/{Module}.Api.csproj.extend/{Entity}.proto.create.md|{Entity}.proto.create]]
