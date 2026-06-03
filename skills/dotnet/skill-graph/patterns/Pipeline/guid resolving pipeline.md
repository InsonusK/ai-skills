---
uid:
status: todo
name: skill-name
description: Resolving entity id when resolve guid request
domain: skill
type: pattern
tags:
  - tag for skill classification
triggers:
  - when skill should called
---
Conversion rule (mandatory)
- No operation is executed directly on GUID.
```
External GUID → always resolved to internal ID first
```
- internal ID resolve BEFORE entering Application Layer or Concurrency validation
# Goal
```
Maximum:
- 3-7 lines
Defines:
- what problem skill solves
- what architectural responsibility it owns
```

## Core Principles
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

## Structure / Contracts
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

## Rules
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

## Anti-patterns

```
Critical section.

AI understands constraints better through:
- forbidden patterns
- invalid examples
- boundary violations

Anti-patterns often provide more value than examples.
```

## Minimal examples only

```
Examples should:
- demonstrate shape
- demonstrate intent
- not teach framework

Good example:
- 5-20 lines

Bad example:
- 100-line tutorial
```