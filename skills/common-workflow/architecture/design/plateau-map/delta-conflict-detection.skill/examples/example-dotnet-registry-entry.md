# Worked example: `{Command}.cs` in `skills/dotnet/architecture`

Four solutions in `skills/dotnet/architecture` carry the `element/command-cs` tag inside their `Implementation/` files: `solution-mediator-integration` (`.create`), `solution-entity-concurrency-change`, `solution-entity-edit-timestamp`, and `solution-external-created-entity` (each `.extend`).

Applying the [5-step workflow](skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#the-5-step-workflow) required actually reading each `.extend.md`'s `## MUST` section, not assuming they compose cleanly:

- `solution-entity-concurrency-change` only ever touches Update/Patch commands (adds a version token, no position requirement) — independent of the other two, `FMN`.
- `solution-external-created-entity` requires `Guid` be the **first property** on Create commands.
- `solution-entity-edit-timestamp` requires `ActionTimeStamp` be the **first property** on Create commands too.

The last two genuinely conflict: an entity that is both an External kind and has edit-timestamp adopted needs both properties on its Create command, but only one can literally be first — `FMC` (no constraint between them, a code change, and a real conflict, not a formal-only one, failing [the wrap/relocate footnote test](skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#the-wraprelocate-footnote-fmc-vs-fmn) since neither solution's author could resolve it alone). This was not obvious from the course's abstract example; it only surfaced from reading the real `.extend.md` files' rules side by side.

The catalog resolved it without a separate resolver solution: the base `solution-mediator-integration` was edited to declare one fixed append order (business fields → `Guid` → `ActionTimeStamp` → version token, each present only when its solution applies), and both `solution-external-created-entity` and `solution-entity-edit-timestamp` were edited to append at their slot instead of each claiming "must be first" — the "collapse to independent" shape from [FMC resolution](skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#fmc-resolution).

See the real, complete entry at [skills/dotnet/architecture/registry/command-cs.md](skills/dotnet/architecture/registry/command-cs.md).
