---
name: fix cecil built_on_plateau floor
description: Correct solution-cecil-architecture-tests' built_on_plateau to match the actual floor required by its own depends_on
problem: solution-cecil-architecture-tests declared built_on_plateau plateau-stateless-non-interactive-service while depending on solution-domain-rules, whose own built_on_plateau is the later plateau-service-with-validated-module-interaction — the two solutions only ever co-exist inside plateau-shared-rules
decision: Set solution-cecil-architecture-tests' built_on_plateau to plateau-service-with-validated-module-interaction, matching solution-domain-rules' own floor
tags:
  - solution/cecil-architecture-tests
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Building `skills/dotnet/architecture/v3`'s Variability Map required tracing every solution's real dependency floor, since [[skills/common-workflow/architecture/design/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] derives a VP's Constraint from `depends_on`/`built_on_plateau` evidence. `solution-cecil-architecture-tests` declared `built_on_plateau: plateau-stateless-non-interactive-service`, but its own `depends_on` names `solution-domain-rules`, whose `built_on_plateau` is the later `plateau-service-with-validated-module-interaction`. A solution cannot coherently claim a shallower floor than a solution it itself requires — `solution-domain-rules` is not composed until `plateau-service-with-validated-module-interaction` (and everything built on top of it, e.g. `plateau-shared-rules`, where both solutions actually appear together in `created_by`). Left uncorrected, the stale floor would have been carried into the Variability Map's Constraint column as if it were accurate.

# Selected variant
[[#Raise built_on_plateau to match the dependency floor (selected)]]

# Searched variants

## Raise built_on_plateau to match the dependency floor (selected)

### Description
Set `solution-cecil-architecture-tests`' `built_on_plateau` to `plateau-service-with-validated-module-interaction` — the same floor `solution-domain-rules` itself declares, and the shallowest plateau consistent with the fact that `solution-cecil-architecture-tests` cannot be meaningfully applied without `solution-domain-rules` already available.

### Benefits
- Removes the inconsistency: a solution's `built_on_plateau` can no longer be shallower than a solution it `depends_on`.
- Matches where both solutions are actually composed together in practice — `plateau-shared-rules`'s `created_by` already lists both side by side.
- No change to `depends_on` itself, no schema change anywhere — a one-field correction.

### Costs
- None identified; this was a straightforward stale value with no design trade-off attached.

## Leave built_on_plateau as plateau-stateless-non-interactive-service

### Description
Keep the original, shallower value unchanged.

### Benefits
- No file change needed.

### Costs
- The field would keep claiming this solution is applicable before its own dependency (`solution-domain-rules`) is even composed — a reader trusting `built_on_plateau` alone would be misled about when this solution is actually usable, and the Variability Map's Constraint column would encode a false floor.
