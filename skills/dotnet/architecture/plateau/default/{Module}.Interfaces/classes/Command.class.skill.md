---
uid: 00b8e3ff-0f48-4da5-ad95-fae927430124
name: command-class
description: Create command implements IHasGuid
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change.solution.skill]]"
  - "[[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration.solution.skill]]"
---

# Goal
- Add `Guid` as a required property on create commands for externally-created entity types
- Implement `IHasGuid` alongside `ICommand<Result<T>>`
- Require all update and patch commands to implement `IHasVersions`
- Make `Versions` a standard property on every command that modifies an existing entity
- Express a named write intent as an immutable record that carries all input needed for the operation
- Implement `ICommand<Result<T>>` so the MediatR pipeline routes it to the correct handler and activates write-side behaviors

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Core Principals
- `Guid` is the first property — signals to the reader that this is an external-created entity
- Command carries the client-generated Guid — never a server-generated value
- Command implements `IHasGuid` from Shared — `{Module}.Interfaces` does not reference BuildingBlocks
- Result record unchanged from command-integration.solution.skill — still just `{Entity}Id`
- Both 201 Created and 409 Conflict return the same response type (`Result<Create{Entity}Result>`)
- `Versions` property typed as `IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>>`
- Populated by the API controller from the decoded `If-Match` header — never hardcoded
- Create and delete commands do NOT implement `IHasVersions` — only update and patch
- Declared as `record` — immutable, structural equality by default
- Implements `ICommand<Result<{CommandName}Result>>` — return type is always `Result<T>`
- Properties are primitives or simple value types — no domain entity references
- Result record declared in the same file — named `{CommandName}Result`
- One command per write intent

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Create command | `Create{Entity}Command` | `CreateTaskCommand` | `Create{Entity}Command.cs` | `CreateTaskCommand.cs` |
| Update command | `Update{Entity}Command` | `Update{Entity}Command` | `Update{Entity}Command.cs` | `Update{Entity}Command.cs` |
| Patch command | `Patch{Entity}Command` | `Patch{Entity}Command` | `Patch{Entity}Command.cs` | `Patch{Entity}Command.cs` |
| Create entity | `Create{Entity}Command` | `CreateTaskCommand` | `{Command}.cs` | `CreateTaskCommand.cs` |
| Update entity | `Update{Entity}Command` | `UpdateTaskCommand` | `{Command}.cs` | `UpdateTaskCommand.cs` |
| Delete entity | `Delete{Entity}Command` | `DeleteTaskCommand` | `{Command}.cs` | `DeleteTaskCommand.cs` |
| Domain action | `{Verb}{Entity}Command` | `AssignTaskCommand` | `{Command}.cs` | `AssignTaskCommand.cs` |
| Command result | `{CommandName}Result` | `CreateTaskResult` | same file as command | `CreateTaskCommand.cs` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Implementation
Create command extended with `Guid` and `IHasGuid`:

```csharp
// {Module}.Interfaces/Commands/Create{Entity}Command.cs
public record Create{Entity}Command(
    Guid Guid,         // ← client-generated, first property
    // ... other properties
) : ICommand<Result<Create{Entity}Result>>, IHasGuid;

public record Create{Entity}Result(int Id);
```

Update command extended with `IHasVersions`:

```csharp
// {Module}.Interfaces/Commands/Update{Entity}Command.cs
public record Update{Entity}Command(
    int {Entity}Id,
    string Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

Patch command similarly:

```csharp
// {Module}.Interfaces/Commands/Patch{Entity}Command.cs
public record Patch{Entity}Command(
    int {Entity}Id,
    string? Title,
    IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>> Versions
) : ICommand<Result>, IHasVersions;
```

Command and result declared together in one file:

```csharp
// {Module}.Interfaces/Commands/CreateTaskCommand.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Commands;

public record CreateTaskCommand(
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```

```csharp
// {Module}.Interfaces/Commands/AssignTaskCommand.cs
using Ardalis.Result;
using Shared.MediatR;

namespace {Module}.Interfaces.Commands;

public record AssignTaskCommand(
    int TaskId,
    int AssigneeId
) : ICommand<Result>;
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Rules
MUST:
	- `Guid` is the first property on the command record
	- Command implements both `ICommand<Result<T>>` and `IHasGuid`
	- `Guid` typed as `System.Guid` — never `string` or `int`
	- Result record co-located with the command
	- Resolver response type matches `Result<Create{Entity}Result>` exactly
	- All update and patch commands implement both `ICommand<Result>` and `IHasVersions`
	- `Versions` populated from decoded `If-Match` header in controller — never constructed in application code
	- Declared as `record`
	- Implement `ICommand<Result<T>>` or `ICommand<Result>` — never `IRequest<T>` directly
	- Result type declared in the same file as the command
	- Properties are primitives or simple types — no domain entity references
MUST NOT:
	- Update, delete, or internal-create commands implement `IHasGuid`
	- Resolver return a different response type than the command handler
	- Create commands implement `IHasVersions` — new entities have no version
	- Delete commands implement `IHasVersions` — deletion does not require version check in this architecture
	- Command contain methods or logic
	- Command reference domain entity types as properties

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Anti-patterns
- `Guid` not as first property — signals external-created entity at a glance
- Resolver response type different from command handler response type — breaks 201/409 symmetry
- `Versions` hardcoded in command constructor call in handler or service

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Check list
- [ ] `Guid` is first property in create command record
- [ ] Command implements `ICommand<Result<T>>` and `IHasGuid`
- [ ] Result record co-located with command
- [ ] Resolver response type matches command handler response type
- [ ] Update command implements `IHasVersions`
- [ ] Patch command implements `IHasVersions`
- [ ] `Versions` passed from controller

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Add Guid as a required property on create commands for externally-created entity types
- [ ] WHEN inspected THEN it implement IHasGuid alongside ICommand<Result<T>>
- [ ] WHEN applied THEN Guid is the first property — signals to the reader that this is an external-created entity
- [ ] WHEN applied THEN Command carries the client-generated Guid — never a server-generated value
- [ ] WHEN applied THEN Command implements IHasGuid from Shared — {Module}.Interfaces does not reference BuildingBlocks
- [ ] WHEN applied THEN Result record unchanged from command-integration.solution.skill — still just {Entity}Id
- [ ] WHEN applied THEN Both 201 Created and 409 Conflict return the same response type
- [ ] WHEN verified THEN Guid is first property in create command record
- [ ] WHEN verified THEN Command implements ICommand<Result<T>> and IHasGuid
- [ ] WHEN naming 'Create command' THEN pattern matches convention
- [ ] WHEN applied THEN Require all update and patch commands to implement IHasVersions
- [ ] WHEN applied THEN Make Versions a standard property on every command that modifies an existing entity
- [ ] WHEN applied THEN Versions property typed as IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>>
- [ ] WHEN applied THEN Populated by the API controller from the decoded If-Match header — never hardcoded
- [ ] WHEN applied THEN Create and delete commands do NOT implement IHasVersions — only update and patch
- [ ] WHEN verified THEN Update command implements IHasVersions
- [ ] WHEN verified THEN Patch command implements IHasVersions
- [ ] WHEN verified THEN Versions passed from controller
- [ ] WHEN naming 'Update command' THEN pattern matches convention
- [ ] WHEN naming 'Patch command' THEN pattern matches convention
- [ ] WHEN applied THEN Express a named write intent as an immutable record that carries all input needed for the operation
- [ ] WHEN inspected THEN it implement ICommand<Result<T>> so the MediatR pipeline routes it to the correct handler and activates write-side behaviors
- [ ] WHEN applied THEN Declared as record — immutable, structural equality by default
- [ ] WHEN applied THEN Implements ICommand<Result<{CommandName}Result>> — return type is always Result<T>
- [ ] WHEN applied THEN Properties are primitives or simple value types — no domain entity references
- [ ] WHEN applied THEN Result record declared in the same file — named {CommandName}Result
- [ ] WHEN applied THEN One command per write intent
- [ ] WHEN naming 'Create entity' THEN pattern matches convention
- [ ] WHEN naming 'Update entity' THEN pattern matches convention
- [ ] WHEN naming 'Delete entity' THEN pattern matches convention
- [ ] WHEN naming 'Domain action' THEN pattern matches convention
- [ ] WHEN naming 'Command result' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/external-created-entity.solution.skill.md|external-created-entity]] - [[skills/dotnet/architecture/solutions/🧩validated/external-created-entity.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/entity-concurrency-change.solution.skill.md|entity-concurrency-change]] - [[skills/dotnet/architecture/solutions/🧩validated/entity-concurrency-change.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.extend.md|{Command}.cs.extend]]
- [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/command-integration.solution.skill.md|command-integration]] - [[skills/dotnet/architecture/solutions/🧩validated/command-integration.solution.skill/Implementation/{Module}.Interfaces.csproj.extend/{Command}.cs.create.md|{Command}.cs.create]]
