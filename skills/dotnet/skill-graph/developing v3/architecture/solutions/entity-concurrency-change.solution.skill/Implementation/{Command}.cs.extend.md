---
description: Update/patch command implements IHasVersions
name: "{Command}.cs"
change_kind: extend
---

# Goals
- Require all update and patch commands to implement `IHasVersions`
- Make `Versions` a standard property on every command that modifies an existing entity

# Core Principles
- `Versions` property typed as `IReadOnlyDictionary<string, IReadOnlyDictionary<int, uint>>`
- Populated by the API controller from the decoded `If-Match` header — never hardcoded
- Create and delete commands do NOT implement `IHasVersions` — only update and patch

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Update command | `Update{Entity}Command` | `Update{Entity}Command` | `Update{Entity}Command.cs` | `Update{Entity}Command.cs` |
| Patch command | `Patch{Entity}Command` | `Patch{Entity}Command` | `Patch{Entity}Command.cs` | `Patch{Entity}Command.cs` |

# Implementation changes

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

# Rules

MUST:
- All update and patch commands implement both `ICommand<Result>` and `IHasVersions`
- `Versions` populated from decoded `If-Match` header in controller — never constructed in application code

MUST NOT:
- Create commands implement `IHasVersions` — new entities have no version
- Delete commands implement `IHasVersions` — deletion does not require version check in this architecture

# Anti-patterns
- `Versions` hardcoded in command constructor call in handler or service

# Check list
- [ ] Update command implements `IHasVersions`
- [ ] Patch command implements `IHasVersions`
- [ ] `Versions` passed from controller
