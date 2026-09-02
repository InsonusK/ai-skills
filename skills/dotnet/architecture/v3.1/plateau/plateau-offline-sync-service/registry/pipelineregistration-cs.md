---
name: registry-pipelineregistration-cs
description: Conflict Detection result for the `pipelineregistration-cs` element in the plateau-offline-sync-service plateau
tags:
  - concern/architecture
  - stack/dotnet
  - element/pipelineregistration-cs
---

# Element
`PipelineRegistration.cs` (`App.Host/DependencyInjection/PipelineRegistration.cs`) — the single `AddPipeline()` method that is the ordered list of MediatR pipeline behaviours.

# Involved solutions
- [[../../../solutions/solution-pipeline-registration.skill/solution-pipeline-registration.skill.md|solution-pipeline-registration]] (`.create` — the empty `AddPipeline()` extension point)
- [[../../../solutions/solution-mediator-exception-handler.skill/solution-mediator-exception-handler.skill.md|solution-mediator-exception-handler]] (`.extend` — prepends `ExceptionHandlingBehavior`, position 1)
- [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]] (`.extend` — inserts `ValidationBehavior` after the exception handler)
- [[../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] (VP5, `.extend` — inserts `ConcurrencyBehavior` after validation)
- [[../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] (VP6, `.extend` — inserts `GuidResolvingBehavior`)
- [[../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] (VP2, `.extend` — appends `UnitOfWorkBehavior` last)

# Classification
`FMN` (from [[../../../delta-conflict-analysis.md#pipelineregistration-cs|delta-conflict-analysis.md]]). Constraint `F` (no Feature-Model constraint fixes the sub-order — VP6 does not require VP5). Category `M` (code change). Kind `N` (independent): each solution inserts its own registration at a documented position and never rewrites another's line. The only coordination need is the execution order.

# Ordering
`source: ordering-only` — the positions are an execution-order requirement, not a Feature-Model constraint. Full order in `AddPipeline()`:

| # | Behaviour | Owner | Position rule |
|---|-----------|-------|---------------|
| 1 | `ExceptionHandlingBehavior` | mediator-exception-handler | first — wraps everything, so no outer behaviour's exception escapes |
| 2 | `ValidationBehavior` | validation-behavior | before any behaviour that assumes a validated request |
| 3 | `ConcurrencyBehavior` | entity-concurrency-change (VP5) | after validation; before the commit |
| 4 | `GuidResolvingBehavior` | external-created-entity (VP6) | after `ConcurrencyBehavior` **when VP5 is applied**, else after `ValidationBehavior`; always before the commit |
| 5 | `UnitOfWorkBehavior` | unit-of-work (VP2) | last — commits only a fully-guarded, non-stale, non-duplicate handler's staged changes |

# Resolution
**Canonical — no resolver.** Each solution's `PipelineRegistration.cs.extend.md` states its own position relative to the others in prose; `GuidResolvingBehavior`'s "after `ConcurrencyBehavior` when VP5 is applied, else after validation" is a conditional both solutions already document. The plateau-offline-sync-service `class-pipeline-registration` structure skill records the composed order, and the example's `PipelineRegistration.AddPipeline()` realises it.

# Architectural signal
Six solutions register into this one method (N≥3). But the group is `FMN`, not `TMC`/`FMC`/`FDC` — the delta-conflict-detection architectural-signal note applies only to the conflicting codes. Each behaviour is a disjoint, position-documented insertion; no VP-boundary reconsideration is warranted.
