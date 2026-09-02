---
name: registry-command-cs
description: Conflict Detection result for the `command-cs` element in the plateau-offline-sync-service plateau
tags:
  - concern/architecture
  - stack/dotnet
  - element/command-cs
---

# Element
`{Command}.cs` (`{Module}.Interfaces/Commands/{Command}.cs`)

# Involved solutions
- [[../../../solutions/solution-mediator-integration.skill/solution-mediator-integration.skill.md|solution-mediator-integration]] (`.create` — base `record` implementing `ICommand<Result<T>>` / `ICommand<Result>`; **owns the fixed property order**)
- [[../../../solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] (VP6, `.extend` — adds `Guid` / `IHasGuid` to a Create command of an external-created entity)
- [[../../../solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] (VP7, `.extend` — adds `ActionTimeStamp` / `ICommandWithTimestamp` to a Create/Update command of a user-initiated entity)
- [[../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] (VP5, `.extend` — adds a version token / `IHasVersions` to an Update/Patch command of a concurrency-controlled entity)

# Classification
Two groups on this element (from [[../../../delta-conflict-analysis.md#command-cs|delta-conflict-analysis.md]]):

1. **`solution-entity-concurrency-change` vs. the other two** — `FMN`. Constraint `F` (no Feature-Model constraint between VP5 and VP6/VP7). Category `M` (code change). Kind `N` (independent) — VP5 only ever touches Update/Patch commands and never claims a property position.
2. **`solution-external-created-entity` (VP6) vs. `solution-entity-edit-timestamp` (VP7)** — `FMC`. Constraint `F` (VP6 and VP7 are independently optional per the Variability Map). Category `M` (code change). Kind `C` (conflicting): an entity that is both External *and* user-initiated has a Create command that both solutions would each place a property on — a positional `record` has one first slot.

# Ordering
`source: ordering-only` — no Feature-Model constraint exists between VP5, VP6, VP7. The order exists purely so every solution knows where to append. It is declared **once**, in `solution-mediator-integration`'s `{Command}.cs.create.md`:

1. **Business fields** — the command's own meaningful inputs.
2. `Guid` (VP6) — present only on a Create command of an external-created entity.
3. `ActionTimeStamp` (VP7) — present only on a command of a user-initiated entity.
4. Version / concurrency token (VP5) — present only on an update command of a concurrency-controlled entity.

Each of 2–4 appears only when its VP applies; no solution claims a fixed absolute position — each appends at its slot.

# Resolution
**Canonical — resolved by convention, no resolver solution.** `solution-external-created-entity` and `solution-entity-edit-timestamp` were each edited to append at their slot (dropped any "must be first" wording); `solution-mediator-integration`'s `{Command}.cs.create.md` states the sub-order and points here. The plateau-offline-sync-service example demonstrates it: `AddItemCommand(SoftItemTitle Title, Guid Guid, DateTimeOffset ActionTimeStamp)` — business field, then Guid, then ActionTimeStamp.

# Architectural signal
Four solutions intersect on this element (N≥3, `FMC` group). Per delta-conflict-detection's rule this is also a reason to reconsider the involved VPs' boundaries — whether `{Command}.cs`'s per-entity-kind property set (`Guid`, version, `ActionTimeStamp`) should eventually be one composed value object instead of three independently-extending solutions each claiming a property slot. Not acted on: the fixed convention is a stable, zero-cost resolution and the VPs are genuinely independent axes.
