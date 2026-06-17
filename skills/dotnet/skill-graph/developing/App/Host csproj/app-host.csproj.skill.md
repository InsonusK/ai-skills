---
uid:
name: skill-name
description: Describe what skill define
domain: skill
type: template
version: 20260606
tags:
  - skill/template/csproj
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

# Structure
## Solution place
`Where defined place in solution`
```
Where does it store in solution
/src
	/ProjectName
```
## Structure
```
What is project structure
```
```
/ProjectName
	/DirectoryName
		ClassesInDirectory.cs
	ProjectName.csproj
```

## Directory and class skills

| Directory or files in directory | Description                                    | Pattern skill          |
| ------------------------------- | ---------------------------------------------- | ---------------------- |
| /DirectoryName                  | Directory description                          | link to folder pattern |
| ClassInDirectory.cs             | Description of class inside of directory above | link to file patter    |
## What Does NOT Belong Here
- component - where it should be
## Allowed Dependencies
```
dependency which allowe to cspproj
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