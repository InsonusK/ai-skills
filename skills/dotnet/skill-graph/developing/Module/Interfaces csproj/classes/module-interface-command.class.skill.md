---
uid: f31ee2da-0acc-431d-87ef-25df4c00f697
name: command
description: defines how to declare a command contract in the Interfaces project
domain: skill
type: template
version: 20260609
tags:
  - skill/template/class
  - dotnet
  - cqrs
  - command
  - mediatr
triggers:
  - declare command
  - create command contract
  - write intent contract
aliases:
  - "{ModuleName}.Interfaces/Commands"
---
# Goal
Define how to declare a MediatR command in [[skills/dotnet/skill-graph/developing/Module/Interfaces csproj/module-interface.csproj.skill|{ModuleName}.Interfaces]]. A command expresses write intent — it carries the input data needed to perform one state-changing operation. The declaration lives in Interfaces so any module can dispatch it without referencing the handler.

# Core Principles
- Command is a declaration only — no logic, no validation
- Implements `ICommand<Result<T>>` — activates `UnitOfWorkBehavior` in pipeline
- `record` type — immutable input contract
- Result type declares what the handler returns on success
- Validation belongs in Application validator — not here

# Governed by
```list of solution which effect on class
- link - 
```

# Structure
## Place in csproj
Defined in `link to project skill`
```
/ProjectName
/Folder
		classByPattern.cs
	ProjectNams.csproj
```
## Naming convention
```example
- class name
	- rule: CommandName + Validator suffix
	- pattern: {CommandName}Validator
	- example: DoTaskValidator
- file name:
	- rule: CommandName + .Validator.cs
	- pattern: {CommandName}.Validator.cs
	- example: DoTask.Validator.cs 
```

## Implementation
```csharp
// Task.Interfaces/Commands/CreateTaskCommand.cs
public record CreateTaskCommand(
    Guid Guid,
    string Title,
    int AssigneeId
) : ICommand<Result<CreateTaskResult>>;

public record CreateTaskResult(int Id);
```

# Rules
```
Defines:
- MUST
- SHOULD
- MUST NOT

Prefer:
- MUST:
	- ...
- MUST NOT:
	- ...

instead of long prose.
```

# Anti-patterns

```
Critical section.

AI understands constraints better through:
- forbidden patterns
- invalid examples
- boundary violations

Anti-patterns often provide more value than examples.
```

# Check list
```
what must be true before this pattern is considered correctly applied?
```

# Unittest TestCases
```
Name cases by boundary, not just happy/sad path. Format: When [context] Then [outcome]
```

# Relations
```
- links to related skill - reason for relation
```

