---
uid:
name: skill-name
description: rules for implementing EF Core entity type configurations for cross module relations
domain: skill
type: template
version: 20260609
tags:
  - skill/template/class
triggers:
  - when skill should called
aliases:
  - App.Infrastructure EF configuration
---
[[skills/dotnet/skill-graph/developing/Architecture/solution/cross-module-communication.solution.skill]]
# Goal
```
Maximum:
- 3-7 lines
Defines:
- what problem skill solves
- what architectural responsibility it owns
```

# Core Principles
```
Most important section.
Defines:
- invariants
- mental model
- architecture decisions

Example:
- Rules define business predicates. 
- Entities define consistency.
- Validators define transport correctness.

This section gives more value than 50 examples.
```

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
```
Defines:
- interfaces
- filesystem structure
- naming conventions
- dependency contracts

Should be:
- compact
- declarative
- predictable
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