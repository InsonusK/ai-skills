---
description: Update/patch command implements IHasVersions
project_name: "{Module}.Interfaces"
name: "{Command}.cs"
element_kind: class
change_kind: extend
tags:
  - solution/entity-concurrency-change
  - element/command-cs
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

# Rule changes

## MUST
- All update and patch commands implement both `ICommand<Result>` and `IHasVersions`
- `Versions` populated from decoded `If-Match` header in controller — never constructed in application code
- Never create commands implement `IHasVersions` — new entities have no version
- Never delete commands implement `IHasVersions` — deletion does not require version check in this architecture

## SHOULD
- Avoid `Versions` hardcoded in command constructor call in handler or service

# Check list
- [ ] Update command implements `IHasVersions`
- [ ] Patch command implements `IHasVersions`
- [ ] `Versions` passed from controller

# Unittest TestCases
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
