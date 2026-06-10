---
uid:
order: 
name: skill-name
description: Short description of skill goal
domain: skill
type: architecture
version: 20260611
tags:
  - skill/architecture/solution
  # any other tags
triggers:
  #What kind of task should agent do to use this solution  
  #- when skill should called
creates:
	#List of classes or project which created by this solution
  #They will have this solution as their created_by
  #Example:
  #- "[[Link]]"
extends:
  #List of classes or project which extended or effected by this solution
  #They will have this solution in their extended_by
  #Example:
  #- "[[Link]]"
depends_on:
  #List of other architecture solutions which is used by this solution
  #Example:
  #- "[[Link]]"
---
**AUTHORING RULE**: 
- Writing solution create folder with name {SolutionName}.solution.skill and add this template into this folder with name {SolutionName}.solution.skill.md
- Replace all ```hint```, ```example``` and ```code example``` blocks with real content. Do not keep them in the final skill file.
- Header property `depends_on` couldn't have links to solution with order is greater or equal order in this solution. If it happend ask user to solve this problem.

# Goal
```hint
List of goals that are pursued by the creation of this solution.
RECOMENDATION:
- Prefer bullet list
```
```example
- Define the system-level architecture for domain events — how events are raised, persisted, and dispatched across the application
```

# Core Principals
```hint
Core principalse that a solution should follow
RECOMENDATION:
- Prefer bullet list
```
```example
- Rules define business predicates
- Entities define consistency.
```

# Requirements
```hint
List of requirements for solution appling
MUST:
- couldn't have link to solution with order number greater that order number of this solution. If it happend ask user to solve this problem.
RECOMENDATION:
- Prefer bullet list
```
```example
- definition of `interface IHasGuid` - solution [[external guid pattern solution]] define IHasGuid interface
```

# Template Skill Mutations
```hint
Create folder "Implementation" into skill folder
All changes which must be made to implements this solution must be writen into folder "Implemetation" using [template from Implemetaion Template](./Implementation Templates/)
Add links to created files below
```
```Example
REPOSITORY
- [[./Implementation Templates/Repository.changes.md|Repository changes]]
PROJECT
- [[./Implementation Templates/Project.changes.md|{File property name}]] - {File property change_kind} - {File property description}
	- [[./Implementation Templates/Class.changes.md|{File property name}]] - {File property change_kind} - {File property description}
- [[./Implementation Templates/Project.changes.md|{File property name}]] {File property change_kind} - {File property description}
	- [[./Implementation Templates/Class.changes.md|{File property name}]] {File property change_kind} - {File property description}
	- [[./Implementation Templates/Class.changes.md|{File property name}]] {File property change_kind} - {File property description}
```

# Rules
```hint
define MUST, SHOULD, SHOULD NOT, MUST NOT rules
```
```example
MUST:
	- ...
SHOULD:
	- ...
SHOULD NOT:
	- ...
MUST NOT:
	- ...
```

# Anti-patterns
```hint
What mean that soltion applyed wrong. 
RECOMENDATION:
- Prefer bullet list
```
```example
- Domain service duplicates invariant already enforced in entity setter or method
```

# Check list
```hint
what must be true before this solution is considered correctly applied?
RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] `int Id` with `internal set` present in Entity
```

# Unittest TestCases
```hint
list of unittests which must be created to test solution. 
RECOMENDATION:
- Prefer checkbox list
- Prefer integration tests
```
```example
- [ ] WHEN call command with event THEN
	- [ ] event fill domain event in entity
	- [ ] `DomainEventInterceptor` catch `SaveChanges` and add event to `outbox`
	- [ ] `OutboxDispatcher` read `outbox` and send `Notification`
```
