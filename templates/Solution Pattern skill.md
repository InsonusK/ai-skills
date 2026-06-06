---
uid:
name: skill-name
description: Describe what skill define
domain: skill
type: pattern
tags:
  - tag for skill classification
  - skill/pattern/solution
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

# Minimal examples only

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