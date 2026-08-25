---
name: rebind-to-statefull-service-parent
description: Why plateau-shared-rules composes plateau-statefull-service as its parent instead of plateau-service-with-validated-module-interaction, and why that means shared-rules now requires persistence.
problem: plateau-shared-rules could be composed on top of plateau-service-with-validated-module-interaction (no persistence) or plateau-statefull-service (with persistence) — either is a valid base for "centralize a duplicated rule". Building it on top of both, as two separate plateaus (service-with-interaction-and-rules, statefull-and-rules), was starting to produce a diamond in the plateau lineage that would only get worse as more optional capabilities were added.
decision: Rebind plateau-shared-rules's parent_plateaus to plateau-statefull-service only. shared-rules is now always layered after persistence, never before it — the lineage stays a single line (stateless -> service-with-interaction -> statefull-service -> shared-rules), not a lattice.
tags:
  - plateau/shared-rules
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`plateau-shared-rules` centralizes a business predicate once it is found duplicated across a VO, an Entity, a PropertyValidator, and a DTO/Command validator (`solution-domain-rules`). Nothing about that capability strictly requires persistence — it was originally composed on top of `plateau-service-with-validated-module-interaction`, the shallowest plateau where a validator/Entity pair could exist at all.

But `{Feature}Check` (from `solution-dto-property-validators`) only gets a real, non-throwing `Load` once `solution-repository-integration` is composed — that happens in `plateau-statefull-service`, a plateau `plateau-shared-rules` does not include. This produced the same forward-reference smell already fixed once for `plateau-service-with-validated-module-interaction` and `plateau-statefull-service` directly (see [[../../../../solutions/solution-dto-property-validators.skill/adr/defer-feature-check-loading-to-persistence-solution.md|solution-dto-property-validators' own ADR]]): `plateau-shared-rules`'s copy of `{Feature}Check` still threw `NotSupportedException`, and `solution-domain-rules`'s `.Check()` redirect had nothing genuinely persisted to prove against.

Building a second copy of `plateau-shared-rules` on top of `plateau-statefull-service` (so both a "with persistence" and a "without persistence" flavor of rule-centralization exist) would have solved that instance, but starts exactly the diamond this ADR exists to avoid:

```
stateless -> service-with-interaction ------------ statefull-service
                        \                                    \
             service-with-interaction-and-rules ---- statefull-and-rules
```

Every future optional capability composed the same way (logging is not this — see [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]] — but a capability that, like rule-centralization, genuinely needs to touch module-internal files) would multiply the same way, and nothing in the current plateau/solution mechanism collapses that multiplication automatically (see the researched prior art: Orthogonal Variability Model, delta-oriented programming, GenVoca/AHEAD — a proper general fix separates the variability model from the plateau tree, but that redesign is out of scope for this pass).

# Selected variant

**Selected variant:** [[#Rebind parent_plateaus to plateau-statefull-service only]]

`plateau-shared-rules`'s `parent_plateaus` now points at `plateau-statefull-service` only. The lineage collapses to a single line:

```
plateau-stateless-non-interactive-service
  -> plateau-service-with-validated-module-interaction
    -> plateau-statefull-service
      -> plateau-shared-rules
```

- `{Feature}Check`'s `Load` is real in `plateau-shared-rules` now — inherited concretely from `plateau-statefull-service`, not re-derived — so `solution-domain-rules`'s `.Check()` redirect is genuinely provable end to end, not just against a mocked/illustrative loading step.
- No second "shared-rules for stateless services" plateau exists or needs to be maintained.
- The trade-off is explicit and accepted: `plateau-shared-rules` now requires persistence. A service that wants centralized rules but genuinely has no persistence is no longer served by this plateau as-is — it would need `solution-domain-rules` applied directly as a solution against `plateau-service-with-validated-module-interaction`, following [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md|plateau-update-by-solutions]] on a case-by-case basis, rather than picking a ready-made plateau.

# Searched variants

## Keep both parents — service-with-interaction-and-rules and statefull-and-rules as two plateaus

### Costs
- Materializes exactly the diamond in the Goal above. Every subsequent optional, module-touching capability doubles the same way, and nothing in `plateau-create-by-solutions`/`solution-plateau-hierarchy` today collapses that combinatorial growth automatically.
- Two plateaus to keep in sync by hand every time `solution-domain-rules` (or anything either parent contributes) changes.

## Give solution-repository-integration's {Feature}Check.cs.extend.md an application condition solution-domain-rules can check for

### Description
Instead of rebinding the plateau, keep `plateau-shared-rules` on `plateau-service-with-validated-module-interaction` and have `solution-domain-rules`'s own `{Feature}Check.cs.extend.md` conditionally redirect only if persistence happens to also be composed.

### Costs
- The plateau/solution composition mechanism has no notion of a conditional `.extend.md` today — building one is exactly the delta-oriented-programming-style "application condition" mechanism identified as future work, not a small fix. Introducing it ad hoc, for one file, without designing it properly for the whole catalog, was rejected as premature — see [Problem](#problem).

## Full redesign now (orthogonal variability model / delta-oriented programming)

### Costs
- Explicitly out of scope for this pass, per the user's own direction: finish the current Solution/Plateau/Plateau-Component concept first, redesign separately once real prior art (OVM, delta-oriented programming, GenVoca/AHEAD) has been studied.
