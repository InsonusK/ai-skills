---
element: command-cs
source: ordering-only
plateau: offline-sync-service
solutions:
  - solution-mediator-integration
  - solution-external-created-entity
  - solution-entity-edit-timestamp
  - solution-entity-concurrency-change
---

# command-cs — ordering-only registry entry

Several solutions append a property to a create/update command record. The **relative order is
fixed once** in `solution-mediator-integration`'s `{Command}.cs.create.md`; no solution claims a
fixed absolute position ("first"):

1. **Business fields** — the command's own meaningful inputs.
2. `Guid` — client-generated id (VP6, `solution-external-created-entity`), on a Create command of an external-created entity.
3. `ActionTimeStamp` — user action time (VP7, `solution-entity-edit-timestamp`), on a command of a user-initiated entity.
4. Version / concurrency token (VP5, `solution-entity-concurrency-change`), on an update command of a concurrency-controlled entity.

Each of 2–4 appears only when its VP applies. This is an **ordering-only** entry: it is not a
Feature-Model constraint and needs no resolver solution — the [delta-conflict analysis](../../../delta-conflict-analysis.md#command-cs)
classifies the VP6×VP7 interaction as `FMC` **resolved by this convention**. In the example,
`AddItemCommand(SoftItemTitle Title, Guid Guid, DateTimeOffset ActionTimeStamp)` — business field, then Guid, then ActionTimeStamp.
