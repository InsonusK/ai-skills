---
uid:
name: skill-name
description: Short description of skill goal
domain: skill
type: architecture
version: 20260612
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
  #List of other architecture solutions which is used by this solution and must be implemented before this solution
  #Example:
  #- "[[Link]]"
adr:
  #List of architecture decision records which was made due to this solution
  #Example:
  #- "[[Link]]"
---
# How Apply this template
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

# Adr
```hint 
If there were architecrute decision record while build solution or edit solution.
1. Add folder ADR in solution solder
2. Add ADR record using [[./adr/adr.template.md]]
3. Add created adr into list of architecrute decision record andvariant which was choosen.
RECOMENDATION:
- Prefer bullet list
```
```examle
- [[Adr link|Adr property name]] 
  - bulets from [[Adr link#Selected variant]]
```

# Requirements
```hint
List of requirements for solution appling and Nuget packages. Define what solution uses from dependensies
RECOMENDATION:
- Prefer bullet list
- Use [[Link|Property Name]] format in link

TEMPLATE:
SOLUTION:
- [[DependencySolution.solution.skill.md|{name}]]
  - [[LinkToProject.csproj.{change_kind}.md|{name}]]
    - [[ProjectClass.class.{change_kind}.md|{name}]] - description how does it used in solution
NUGET
- {Nuget package name} {version}
  - {Classs} - description how does it used in solution
```
```example
SOLUTION:
- [[repository-structure.solution.skill.md|Repository structure solution]]
  - [[app-host.csproj.extended.md|App.Host]]
    - [[command.class.created.md|Command]] - add extension `IRequest` to `Command` classs
NUGET
- IMediatR
  - IRequest - added to `Command` class
```

# Template Skill Mutations
```hint
1. Create folder "Implementation" into skill folder
2. All changes which must be made to implements this solution must be writen into folder "Implemetation" using [template from Implemetaion Template](./Implementation Templates/)
3. Implementation file naming rule
	1. For Repository.template - Repository.{File property change_kind}.md
	2. For Project.template - {ProjectName}.csproj.{File property change_kind}.md
	3. For Class.template - {ClassName}.cs.{File property change_kind}.md
4. Implementation file must be put into folder "Implemetation" following this structure
- Implementation
-- Repository.{File property change_kind}.md
-- {ProjectName}.csproj.{File property change_kind}.md
-- /{ProjectName}.csproj.{File property change_kind}
--- {ClassName}.cs.{File property change_kind}.md
ATTENSION for dynamic names like Module name or Entity name. Prefer using {Module} or {Entity} notation. It shows that Module or Entity is not constant name.
5. Every solution skill must provide concrete implementation files, including classification, decision, policy, or taxonomy skills. If the skill selects between variants, provide an implementation file for each variant that shows the resulting code or configuration.
6. When this skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.

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
