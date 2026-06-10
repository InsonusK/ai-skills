---
uid:
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
  #Project fill {ProjectName}.csproj
  #Classes fill {Namespace}.{ClassName}.cs
  #Example:
  #- "App.Host.cspoj"
  #- "App.Host.Program.cs"
  #- "{Module}.Domain.cspoj"
  #- "{Module}.Domain.Entities.Entity.cs"
extends:
  #List of classes or project which extended or effected by this solution
  #Project fill {ProjectName}.csproj
  #Classes fill {Namespace}.{ClassName}.cs
  #Example:
  #- "App.Host.cspoj"
  #- "App.Host.Program.cs"
  #- "{Module}.Domain.cspoj"
  #- "{Module}.Domain.Entities.{EntityName}.cs"
depends_on:
  #List of other architecture solutions which is used by this solution
  #Example:
  #- "[[Link]]"
---
# Who Apply this template
- Writing solution create folder with name {SolutionName}.solution.skill and add this template into this folder with name {SolutionName}.solution.skill.md
- Fill template using 
	- ```hint``` - how template should be filled
	- ```example``` - example of template filling
	- ```code example``` - example how do you need code examples
- Clearing template hints:
	- Remove block "# Template rules"
	- remove all ```hint```, ```example```, ```code example```
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
1. Create folder "Implementation" into skill folder
2. All changes which must be made to implements this solution must be writen into folder "Implemetation" using [template from Implemetaion Template](./Implementation Templates/)
3. Implementation file naming rule
	1. For Repository.template - Repository.{File property change_kind}.md
	2. For Project.template - {ProjectName}.csproj.{File property change_kind}.md
	3. For Class.template - {ClassName}.cs.{File property change_kind}.md
ATTENSION for dynamic names like Module name or Entity name. Prefer using {Module} or {Entity} notation. It shows that Module or Entity is not constant name.

Add links to created files as shown bellow:
REPOSITORY:
- [[./Implementation Templates/Repository.{File property change_kind}.md|Repository]] - {File property change_kind} - {File property description}
PROJECT:
- [[./Implementation Templates/{File property name}.{File property change_kind}.md|{File property name}]] - {File property change_kind} - {File property description}
	- [[./Implementation Templates/{File property name}.{File property change_kind}.md|{File property name}]] - {File property change_kind} - {File property description}
- [[./Implementation Templates/{File property name}.{File property change_kind}.md|{File property name}]] - {File property change_kind} - {File property description}
	- [[./Implementation Templates/{File property name}.{File property change_kind}.md|{File property name}]] - {File property change_kind} - {File property description}
	- [[./Implementation Templates/{File property name}.{File property change_kind}.md|{File property name}]] - {File property change_kind} - {File property description}
```
```Example
REPOSITORY:
- [[./Implementation Templates/Repository.extend.md|Repository]] - extend - add app host
PROJECT:
- [[./Implementation Templates/App.Host.csproj.create.md|App.Host.csproj]] - create - be root of app composition
	- [[./Implementation Templates/DIConfiguration.cs.create.md|DIConfiguration.cs]] - create - Be single point of registration into Service collection
- [[./Implementation Templates/{Module}.Domain.csproj.extend.md|{Module}.Domain.csproj]] - create - core project of domain logic
	- [[./Implementation Templates/{Entity}.cs.extend.md|{Entity}.cs]] - extend - add invariant validation by rules
	- [[./Implementation Templates/Rule.cs.create.md|Rule.cs]] - create - add invariant rules
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
