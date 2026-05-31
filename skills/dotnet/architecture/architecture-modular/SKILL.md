---
name: modular-architecture
description: Defines the global architecture rules for the .NET modular monolith solution.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - architecture
    - architecture-modular
---
## Solution Structure

The solution uses a modular monolith architecture.

Each business module must be isolated.

Each module contains:

* Contracts
* Application
* Domain
* Infrastructure
* Tests

Example:

TaskModule/
- Task.Contracts/
- Task.Application/
- Task.Domain/
- Task.Infrastructure/
- Task.UnitTests/
- Task.IntegrationTests/

## Dependency Rules

Allowed dependencies:

* Contracts -> no internal dependencies
* Application -> Contracts + Domain
* Domain -> no infrastructure dependencies
* Infrastructure -> Application + Domain
* API -> Contracts + Application bootstrap only

Forbidden:

* Domain referencing Infrastructure
* Cross-module Infrastructure access
* API directly accessing DbContext
* Static service locators

## Architectural Style

Use:

* CQRS with MediatR
* Vertical Slice Architecture
* Feature-based organization
* Explicit module boundaries

Avoid:

* Generic repositories
* God services
* Shared mutable state
* Cross-module entity references

## Module Communication

Inter-module communication must happen through:

* MediatR requests
* Contracts
* Integration events

Never directly access another module's DbContext or entities.
