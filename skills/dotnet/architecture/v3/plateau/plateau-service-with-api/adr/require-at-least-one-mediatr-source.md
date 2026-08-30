---
name: require at least one MediatR handler source
description: Encode the already-documented requirement that solution-http-api-publication needs at least one of solution-query-integration/solution-command-integration
problem: solution-http-api-publication's own description and this plateau's Core Principles already state "requires at least one of query-integration or command-integration to provide MediatR handler targets", but solution-http-api-publication's depends_on field was left empty — the constraint existed only in prose
decision: Add solution-query-integration and solution-command-integration to solution-http-api-publication's depends_on, matching what the prose already asserted
tags:
  - solution/http-api-publication
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Building `skills/dotnet/architecture/v3`'s Variability Map ([[skills/common-workflow/architecture/design/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]]) requires deriving every VP's Constraint column from real, checkable evidence — a `depends_on`/`built_on_plateau` edge, or a solution's own stated prose requirement. `solution-http-api-publication`'s `description` states "Requires at least one of query-integration or command-integration to provide MediatR handler targets," and this plateau's own Core Principles restate the same rule — but the solution's `depends_on` field was empty. Recording the Constraint in the Variability Map without also fixing the underlying solution would leave the map and the actual catalog silently out of sync the moment someone reads only the solution file.

# Selected variant
[[#Add both as depends_on entries (selected)]]

# Searched variants

## Add both as depends_on entries (selected)

### Description
Add `solution-query-integration` and `solution-command-integration` to `solution-http-api-publication`'s `depends_on` list. Either one alone already satisfies the "at least one" requirement in practice — MediatR/`plateau-create-by-solutions`' own dependency discovery does not currently model "at least one of N", so both are listed and a module is expected to have applied at least one before `solution-http-api-publication` is meaningful, per the plateau's own Core Principles wording.

### Benefits
- Closes the exact gap this ADR exists to close: the requirement is now visible from the solution file itself, not only from prose a reader might not open.
- No schema change — `depends_on` stays the plain wikilink list every stack's `solution-create` already defines.
- Matches how the plateau's own Core Principles already describe the requirement in words; this ADR only makes it structurally checkable too.

### Costs
- `depends_on` cannot currently express "at least one of these two, not necessarily both" — listing both slightly overstates the requirement as "needs both". Accepted: the alternative (adding an "at least one of" construct to `depends_on`) would be a schema change to `solution-create`, out of scope for this catalog-local fix, and the plateau's own prose already carries the precise "at least one" wording for a reader who needs it.

## Leave depends_on empty, rely on prose only

### Description
Keep the status quo: state the requirement only in `description`/Core Principles, add nothing to `depends_on`.

### Benefits
- No file changes needed.

### Costs
- The exact gap this ADR exists to close remains: a reader or tool inspecting only `depends_on` (as `plateau-create-by-solutions`'s own dependency scans do) has no way to see the requirement, and the Variability Map's Constraint column would have no structural evidence to point to.
