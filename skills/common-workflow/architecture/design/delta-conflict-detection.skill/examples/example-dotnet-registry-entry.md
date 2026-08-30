# Worked example: `{Command}.cs` in `skills/dotnet/architecture/v3`

Four solutions in `skills/dotnet/architecture/v3` carry the `element/command-cs` tag inside their `Implementation/` files: `solution-command-integration` (`.create`), `solution-entity-concurrency-change`, `solution-entity-edit-timestamp`, and `solution-external-created-entity` (each `.extend`).

Applying the [5-step workflow](../delta-conflict-detection.skill.md#the-5-step-workflow) required actually reading each `.extend.md`'s `## MUST` section, not assuming they compose cleanly:

- `solution-entity-concurrency-change` only ever touches Update/Patch commands (adds `Versions`, no position requirement) — independent of the other two, `FMN`.
- `solution-external-created-entity` requires `Guid` be the **first property** on Create commands.
- `solution-entity-edit-timestamp` requires `ActionTimeStamp` be the **first property** on Create commands too.

The last two genuinely conflict: an entity that is both an External kind and has edit-timestamp adopted needs both properties on its Create command, but only one can literally be first — `FMC` (no constraint between them, a code change, and a real conflict, not a formal-only one). This was not obvious from the course's abstract example; it only surfaced from reading the real `.extend.md` files' rules side by side.

Per the parent skill's rule, the resolver this case needs was **not** invented on the spot — the real registry entry records it as an open finding needing an explicit ADR-backed choice (which property goes first), rather than picking arbitrarily.

See the real, complete entry at [skills/dotnet/architecture/v3/plateau/plateau-statefull-service/registry/command-cs.md](../../../../../dotnet/architecture/v3/plateau/plateau-statefull-service/registry/command-cs.md).
