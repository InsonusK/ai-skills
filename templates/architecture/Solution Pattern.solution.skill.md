---
uid:
name: skill-name
description: Describe what skill define
domain: skill
type: architecture
version: 20260606
tags:
  - skill/architecture/solution
  #- tag for skill classification
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

# Affected objects
```
list of effected class patterns - how does class effected
```
# Contracts
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