---
name: restore-sample-api-and-greet-command
description: Why Sample.Api and the GreetCommand chain, both present in the parent plateau's example, were restored to this plateau's example instead of staying silently dropped.
problem: An audit found this plateau's example missing Sample.Api entirely and missing GreetCommand/GreetCommandHandler/its Gherkin coverage, both present in the parent plateau (plateau-stateless-non-interactive-service)'s example, with no ADR or skill text recording either removal as an intentional decision.
decision: Restore Sample.Api as a minimal placeholder project (matching the parent's shape) and restore GreetCommand/GreetCommandHandler plus their Gherkin coverage so they coexist with CreateTaskCommand, rather than treating either omission as acceptable or re-justifying it after the fact.
tags:
  - plateau/service-with-validated-module-interaction
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`plateau-service-with-validated-module-interaction` declares `parent_plateaus: [plateau-stateless-non-interactive-service]`, which per [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] means this plateau's `example/` must evolve the parent's `example/`, not silently drop parts of it. An audit of the built example found two such drops, neither recorded anywhere:

1. **`Sample.Api` was entirely missing.** The parent's example has `src/Modules/Sample/Sample.Api/{Placeholder.cs, Sample.Api.csproj}`, referenced from the parent's `.sln`. This plateau's own `.sln` and `src/Modules/Sample/` had no `Sample.Api` project at all — also independently violating the base MUST rule (inherited from `solution-sln-structure`) that every module has at least the base four projects (Api, Application, Domain, Interfaces).
2. **`GreetCommand`/`GreetCommandHandler` and their Gherkin coverage were dropped wholesale**, replaced outright by `CreateTaskCommand`/`CreateTask.Handler.cs` instead of the two coexisting. `Sample.Domain/Entities/Greeting.cs` was left behind as orphaned dead code — its only caller, `GreetCommandHandler`, had been deleted, so nothing referenced it any more.

Neither removal was disclosed: the plateau root skill's "extends it with" list framed every change as purely additive, and no `adr/` folder existed in this plateau (or in the parent) to record a deliberate exclusion.

# Selected variant

**Selected variant:** [[#Restore Sample.Api and GreetCommand so they coexist with the plateau's own additions (selected)]]

# Searched variants

## Restore Sample.Api and GreetCommand so they coexist with the plateau's own additions (selected)

### Description
Add back `Sample.Api` (identical in shape to the parent's placeholder project — this plateau still introduces no HTTP/gRPC transport, that arrives two plateaus later via `solution-http-api-publication`) and register it in the `.sln`. Restore `GreetCommand.cs`, `GreetCommandHandler.cs`, and the `GreetCommand`/`Greeting`/`GreetCommandShape` Gherkin features and step definitions across `Sample.Interfaces`, `Sample.Application`, and `Sample.Domain` (and their `*.Tests` projects), unchanged from the parent. `Sample.Domain/Entities/Greeting.cs` becomes live code again — it was never actually deleted, only orphaned — since `GreetCommandHandler` calls it. `CreateTaskCommand` remains as this plateau's own new demonstration of Value Objects, `ValidationBehavior`, and cross-module DTO validators, dispatched alongside `GreetCommand` from `App.Host/Program.cs` so both command chains are provably exercised.

### Benefits
- Matches the MUST rule this plateau's own hierarchy already requires: nothing from the parent's example silently disappears.
- Every module keeps its required base four projects (Api, Application, Domain, Interfaces), satisfying `solution-sln-structure`'s base rule.
- `Greeting.cs` stops being orphaned dead code without deleting it — no functionality is lost, and no extra deletion is required.
- `GreetCommand` and `CreateTaskCommand` together give the example two independent, non-conflicting illustrations of the command chain: one minimal (no cross-module validation), one exercising this plateau's actual new capability. No namespace or DI-registration collision exists between them (verified: distinct Gherkin step regexes, distinct MediatR request types, both picked up by the same `AddValidatorsFromAssembly`/`RegisterServicesFromAssembly` scan already in `SampleApplicationRegistration`).

### Costs
- The example carries one extra (intentionally trivial) command chain beyond what's needed to demonstrate this plateau's new capability — a minor increase in example surface area, accepted because the alternative is silently regressing the parent.

## Leave the regression in place and document it after the fact as an intentional removal

### Description
Keep `Sample.Api` and `GreetCommand` out of the example, and instead write an ADR post-hoc declaring their removal to have been deliberate (e.g. "GreetCommand was a toy example superseded by CreateTaskCommand").

### Costs
- Does not fix the independent MUST-rule violation: a module without an Api project still has fewer than the base four projects, regardless of how the omission is documented.
- Retroactively declaring an undocumented regression "intentional" after an audit catches it is not a genuine decision — no alternative was actually weighed at the time the files were deleted, so the ADR would misrepresent history rather than record it.
- Every later plateau built on top (`plateau-statefull-service` and beyond) would keep inheriting the same shrunken module shape, compounding the gap instead of closing it.

## Replace CreateTaskCommand with GreetCommand extended to use Value Objects, instead of keeping both

### Description
Rather than keeping two command chains, retrofit `GreetCommand` itself to demonstrate Value Objects/`ValidationBehavior`/cross-module validators (e.g. give it a `SoftEmail`-typed field), and drop `CreateTaskCommand`.

### Costs
- `GreetCommand`'s shape (a single `Name` string producing a greeting) does not naturally fit an entity-creation, guarded-behavior narrative — bending it to also demonstrate `TaskItem`-style entity behavior would make one example do two unrelated jobs.
- Still requires touching/renaming the parent's `GreetCommand` contract, which is a heavier change than leaving it untouched and additive.
