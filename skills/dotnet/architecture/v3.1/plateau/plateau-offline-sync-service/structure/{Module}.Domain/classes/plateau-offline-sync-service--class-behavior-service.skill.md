---
name: plateau-offline-sync-service--class-behavior-service
description: Class {Behavior}Service in the plateau-offline-sync-service plateau — a static domain-service class of extension methods for bulky entity behavior in {Module}.Domain/Services
whenToUse: when entity behavior logic outgrows the entity method and needs to move to a domain service, or editing an existing one
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
---

# Goal
- Encapsulate bulky or multi-step domain logic that does not fit inside an entity method, while keeping the entity the gatekeeper of its state.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static class` of extension methods on the entity type, in `/{Module}.Domain/Services`.
- Stateless and side-effect free apart from invoking the entity's guarded mutation methods.
- Validates via a condition it owns locally, before calling into the entity.
- Never owns property mutation — the entity exposes a guarded `internal` method the service calls.
- No dependency on EF Core, FluentValidation, ASP.NET, `HttpContext`, or any infrastructure.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-behavior-service
// Plateau: domain-service
// Version: 20260902000000
using Shared.Exceptions;
using {Module}.Domain.Entities;

namespace {Module}.Domain.Services;

public static class {Entity}PricingService
{
    public static void RecalculateTotal(this {Entity} entity, IEnumerable<LineItem> items)
    {
        if (!items.Any())
            throw new DomainException("{Module}.{Entity}.MustHaveLineItem", "…");

        entity.SetTotal(items.Sum(i => i.Quantity * i.UnitPrice)); // entity's guarded internal method
    }
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]] - [[../../../../../solutions/solution-domain-behaviour.skill/Implementation/{Module}.Domain.csproj.create/{Behavior}Service.cs.create.md|{Behavior}Service.cs.create]]

# Rules
MUST:
- Be a `static class` in `/{Module}.Domain/Services`, named for the behavior it encapsulates.
- Validate locally before calling into the entity; mutate entity state only through the entity's guarded methods.
- Hold no instance state; depend on no infrastructure or framework.
- Never introduce a second uncoordinated public mutation point for a property.
- Never apply several plateau templates per class.

# Check list
- [ ] `static class` in `/Services`, extension methods on the entity type.
- [ ] Validates locally; mutates only via the entity's guarded methods.
- [ ] No infrastructure/framework dependency, no instance state.

# Unittest TestCases
- [ ] WHEN the service's condition fails THEN `DomainException` is thrown and no entity method is called.
- [ ] WHEN the condition passes THEN the entity's guarded method is invoked with the computed value.
