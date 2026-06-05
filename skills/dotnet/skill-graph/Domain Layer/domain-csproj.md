---
uid: c3e7b1be-9e3d-4cac-b518-60ccd49dae2b
status: draft
name: skill-name
description: purpose of module domain csproj, boundaries and goal
domain: skill
type: pattern
tags:
  - Domain
  - module
  - csproj
triggers:
  - create domain cs project
aliases:
  - "{Module}.Domain"
---
# Goal
- Define content of module domain csproj
- Define structure of csproj
- Define rules
- Define purpose and boundaries

## Core Principles
- Define Domain model logic

## Structure / Contracts
- /{ModuleName}.Domain
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-configuration-pattern.skill|Configuration]]
	- /[[skills/dotnet/skill-graph/Domain Layer/entity/entity-pattern.skill|Entities]]
	- /[[skills/dotnet/skill-graph/Domain Layer/value-object-pattern.skill|ValueObjects]]
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-rule-pattern.skill|Rules]]
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-service.skill|Services]]
	- /[[skills/dotnet/skill-graph/Domain Layer/domain-event-pattern.skill|Events]]
	- {ModuleName}.Domain.csproj

## Rules
- Must
	- Define Domain entities
	- Define ValueObjects used with entities
	- Define business logic of entities changes
	- Define domain events
	- Define EF configurations for
		- Entity indexes
		- Foreing key inside module
- MUST NOT:
	- define external foreing keys
## Anti-patterns
- Store business workflow

## Minimal examples only
- /TaskModule.Domain
	- /Entites
		- Task.cs
		- TaskRelation.cs
	- /ValueObjects
		- RelationType.cs
		- TaskImportance.cs
		- TaskValue.cs
	- /Service
		- CompleteTaskService.cs
	- /Event
		- TaskCompleted.cs