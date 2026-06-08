---
uid:
name: skill-name
description: Describe what skill define
domain: skill
type: template
version: 20260608
tags:
  - skill/template/class
  - tag for skill classification
triggers:
  - when skill should called
---
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
	- example: {CommandName}Validator
- file name:
	- rule: CommandName + .Validator.cs
	- pattern: {CommandName}.Validator.cs
	- example: {CommandName}.Validator.cs 
```

# Implementation
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
# Result status mapping

| Result                   | HTTP meaning              |
| ------------------------ | ------------------------- |
| `Result.Created(value)`  | 201 — entity created      |
| `Result.Success(value)`  | 200 — operation succeeded |
| `Result.NoContent()`     | 204 — succeeded, no body  |
| `Result.NotFound()`      | 404 — entity not found    |
| `Result.Conflict(msg)`   | 409 — business conflict   |
| `Result.Invalid(errors)` | 400 — validation failed   |
| `Result.Error(msg)`      | 500 — unexpected failure  |

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