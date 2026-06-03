---
uid: 02c1af73-bafc-4072-b453-51e20cd3a345
status: todo
name: skill-name
description: how implement async creation process
domain: skill
type: pattern
tags:
  - entity
  - creation
  - architecture
triggers:
  - develop async ccreation process
---

Описать как работает паттер по асинхронному созданию сущностей
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