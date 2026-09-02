---
element: pipelineregistration-cs
source: ordering-only
plateau: offline-sync-service
solutions:
  - solution-pipeline-registration
  - solution-validation-behavior
  - solution-mediator-exception-handler
  - solution-entity-concurrency-change
  - solution-external-created-entity
  - solution-unit-of-work
---

# pipelineregistration-cs — ordering-only registry entry

`PipelineRegistration.AddPipeline()` is the single ordered list of MediatR pipeline behaviors.
Each behavior solution inserts its registration at a **named position relative to the others**;
the positions are not Feature-Model constraints, only an execution-order requirement:

| # | Behavior | Owner | Rule |
| - | -------- | ----- | ---- |
| 1 | `ExceptionHandlingBehavior` | `solution-mediator-exception-handler` | first — wraps everything |
| 2 | `ValidationBehavior` | `solution-validation-behavior` | before any behavior that assumes a validated request |
| 3 | `ConcurrencyBehavior` | `solution-entity-concurrency-change` (VP5) | after validation; before the commit |
| 4 | `GuidResolvingBehavior` | `solution-external-created-entity` (VP6) | after `ConcurrencyBehavior` **when VP5 is applied**, else after `ValidationBehavior`; before the commit |
| 5 | `UnitOfWorkBehavior` | `solution-unit-of-work` (VP2) | **last** — commits only a fully-guarded handler's staged changes |

VP6 does **not** require VP5; the position of `GuidResolvingBehavior` relative to `ConcurrencyBehavior`
is ordering-only (a duplicate-Guid short-circuit must precede any commit). See the
[delta-conflict analysis](../../../delta-conflict-analysis.md#pipelineregistration-cs).
