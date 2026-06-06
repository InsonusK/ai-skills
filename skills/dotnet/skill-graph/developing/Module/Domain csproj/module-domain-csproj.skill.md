---
uid: c3e7b1be-9e3d-4cac-b518-60ccd49dae2b
name: module-domain-csproj
description: defines the Domain project boundary, its structure, what belongs inside it, and what is forbidden
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - module
  - skill/pattern/csproj
triggers:
  - create domain project
  - what belongs in domain
  - domain layer structure
  - create domain cs project
aliases:
  - "{ModuleName}.Domain"
  - "{ModuleName}.Domain.csproj"
---
# Goal
Define what the `{ModuleName}.Domain` project (`.csproj`) is, what it contains, and what it must never contain. Domain is the innermost layer — it owns business logic, entity definitions, value objects, rules, and events. It has no runtime dependencies on infrastructure, application services, or other modules.

# Core Principles
- Domain owns business logic — no other layer does
- Domain is pure — no infrastructure dependencies at runtime
- Domain never references Application, Infrastructure, or other modules' Domain
- EF Core is the only infrastructure reference allowed — only for `IEntityTypeConfiguration<T>`
- Everything in Domain is owned by this module — no shared domain model across modules

# Solution Place
Defined in [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill#Layers Overview|backend-project-structure.skill]]
# Structure
```
/{ModuleName}.Domain
  /Configurations
  /Entities
  /ValueObjects
  /Rules
  /Services
  /Events
  /Specifications
  {ModuleName}.Domain.csproj
```
Directory and there classes from `{ModuleName}.Domain` belong patterns: 

| Directory       | Description                                       | Pattern skill                                                                                                                                |
| --------------- | ------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| /Configuration  | EF Core entity type configurations                | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-configuration-pattern.skill\|domain-configuration-pattern.skill]] |
| /Entities       | domain entities                                   | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill\|entity-pattern.skill]]                                     |
| /ValueObjects   | value objects                                     | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill\|value-object-pattern.skill]]                 |
| /Rules          | domain rules and predicates                       | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-rule-pattern.skill\|domain-rule.skill]]                           |
| /Services       | domain services for complex or multi-entity logic | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-service.skill\|domain-service.skill]]                             |
| /Events         | domain event definitions                          | [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-event-pattern.skill\|domain-event.skill]]                         |
| /Specifications | simple single-condition Ardalis specs             | [[domain-specification.skill]]                                                                                                               |

# What Does NOT Belong Here
- Command handlers, query handlers — belong in Application
- Validators — belong in Application
- Repository interfaces — belong in BuildingBlocks
- MediatR dispatching — belongs in Application
- DbContext — belongs in App.Infrastructure
- HTTP concerns — belong in Api
- Cross-module foreign key configurations — belong in App.Infrastructure

# Allowed Dependencies
```
{ModuleName}.Domain → Shared
{ModuleName}.Domain → Microsoft.EntityFrameworkCore  (IEntityTypeConfiguration only)
```

# Rules
MUST:
- One `.csproj` per module Domain — never shared
- All entity invariants enforced inside Domain
- EF configurations in `/Configurations` — never annotations on entities
- Domain events defined as `record` implementing `IDomainEvent`
- Simple specs (single condition) live in `/Specifications`
MUST NOT:
- Reference `{ModuleName}.Application` or `App.Infrastructure`
- Reference any other module's Domain
- Use MediatR, repositories, or DbContext at runtime
- Put business workflow orchestration here — that belongs in Application

# Checklist
- [ ] Project has no runtime infrastructure dependencies
- [ ] All folders present: Configurations, Entities, ValueObjects, Rules, Services, Events, Specifications
- [ ] No Application or Infrastructure references in .csproj
- [ ] No cross-module Domain references

# Relations
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/entity.skill|entity.skill]] — entity definitions live here
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/value-object-pattern.skill|value-object.skill]] — value objects live here
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-event-pattern.skill|domain-event.skill]] — event definitions live here
- [[skills/dotnet/skill-graph/developing/Module/Domain csproj/Classes/domain-configuration-pattern.skill|ef-configuration.skill]] — EF configurations live here
- [[skills/dotnet/skill-graph/developing/Module/module-layer.skill|module-layer.skill]] — Domain is one of four module projects
- [[skills/dotnet/skill-graph/developing/Architecture/backend-project-structure.skill|backend-project-structure.skill]] — dependency rules at solution level
