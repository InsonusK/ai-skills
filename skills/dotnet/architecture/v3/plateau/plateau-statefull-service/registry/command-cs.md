---
name: registry-command-cs
description: Conflict Detection result for the `{Command}.cs` element
tags:
  - concern/architecture
  - stack/dotnet
  - element/command-cs
---

# Element
`{Command}.cs` (`{Module}.Interfaces/Commands/{Command}.cs`)

# Involved solutions
- [[skills/dotnet/architecture/v3/solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] (`.create` — base `record` implementing `ICommand<Result<T>>`/`ICommand<Result>`)
- [[skills/dotnet/architecture/v3/solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] (`.extend` — adds `IHasVersions`/`Versions` to Update/Patch commands only; explicitly excludes Create/Delete)
- [[skills/dotnet/architecture/v3/solutions/solution-external-created-entity.skill/solution-external-created-entity.skill.md|solution-external-created-entity]] (`.extend` — adds `Guid`/`IHasGuid` to Create commands only, MUST be the **first property**)
- [[skills/dotnet/architecture/v3/solutions/solution-entity-edit-timestamp.skill/solution-entity-edit-timestamp.skill.md|solution-entity-edit-timestamp]] (`.extend` — adds `ActionTimeStamp`/`ICommandWithTimestamp` to Create/Update commands, MUST be the **first property**)

# Classification
Two separate groups on the same element:

1. **`solution-entity-concurrency-change` vs. the other two** — `FMN`. Constraint: `F` (no Feature-Model constraint between VP1's Mutable variant and edit-timestamp/external-created). Category: `M` (code change). Kind: `N` (independent) — it only ever touches Update/Patch commands and never claims a specific property position, so it cannot collide with either of the other two's position requirements.
2. **`solution-external-created-entity` vs. `solution-entity-edit-timestamp`** — `FMC`. Constraint: `F` (no `depends_on`/constraint declared between these two VPs — both are independently optional per [[skills/dotnet/architecture/v3/variability-map.md|the Variability Map]]). Category: `M` (code change). Kind: `C` (conflicting): for an entity that is both an External kind (VP1) *and* has edit-timestamp adopted, its Create command must satisfy both "`Guid` is the first property" (`solution-external-created-entity`'s own MUST) and "`ActionTimeStamp` is the first property" (`solution-entity-edit-timestamp`'s own MUST) — only one property can literally be first in a positional `record`. This is a real conflict discovered by reading both `.extend.md` files' `## MUST` sections, not a hypothetical one.

# Ordering
No constraint exists between VP1's External variant and edit-timestamp's adoption (`source: ordering-only` if a resolver's `depends_on` is added — see Resolution below). Nothing today fixes which of `Guid`/`ActionTimeStamp` must be declared first.

# Resolution
**Not yet resolved — flagged as an open finding from applying this catalog's Conflict Detection for the first time, not silently decided.** Per [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/delta-conflict-detection.skill.md#FDC resolution|delta-conflict-detection]]'s rule that a resolver is never folded into either original solution, the fix should be a small, separate resolver solution (e.g. `solution-guid-and-timestamp-command-ordering`), `depends_on` both `solution-external-created-entity` and `solution-entity-edit-timestamp`, fixing one canonical property order for the intersection case (an External + edit-timestamp-adopted entity's Create command). Which property goes first is a real design choice — recommend the plateau owner decide it explicitly (with an ADR) rather than have it picked arbitrarily here.

# Architectural signal
Four solutions intersect on this element (N≥3, counting all four listed above). Per the parent skill's rule this is recorded even though only two of the four (`solution-external-created-entity`/`solution-entity-edit-timestamp`) actually conflict — worth reviewing, when the resolver above is built, whether `{Command}.cs`'s per-entity-kind property set (Guid, Versions, ActionTimeStamp) should eventually be modeled as one composed value object instead of three independently-extending solutions each claiming their own property placement.
