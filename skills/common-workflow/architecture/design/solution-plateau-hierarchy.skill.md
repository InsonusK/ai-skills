---
name: solution-plateau-hierarchy
description: Defines the relationship between Solutions and Plateaus — when something qualifies as a Plateau instead of a Solution, how a Plateau composes other Plateaus, and how a Solution builds on top of a Plateau
whenToUse: when deciding whether a new architectural unit should be a solution or a plateau, when a plateau needs to be composed from other plateaus instead of assembled only from solutions, or when a solution needs to declare that it builds on top of an existing plateau instead of on individual solutions inside it
tags:
  - skill/architecture/design
  - stack
  - concern/architecture
---

# Goal
- Give every architectural unit an unambiguous place in one hierarchy — Solution or Plateau — so an agent building a new one knows immediately which shape to use.
- Let a Plateau be composed from other Plateaus, not only assembled from a flat list of Solutions, so a genuinely reusable capability (a validation stack, a persistence stack) is captured once and reused by every larger Plateau that needs it, instead of being re-declared as a `depends_on` on each solution inside it separately.
- Let a Solution declare that it builds on a Plateau as a whole, distinctly from `depends_on` on sibling solutions, for the same reason.

# Core Principle
- A **Solution** ([[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]) is the atomic, reusable mechanism — it may `depends_on` other solutions, but never composes a plateau.
- A **Plateau** ([[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]) is a concretely-functional, agent-facing unit assembled from solutions and/or other plateaus. Whether something qualifies as a plateau is not a question of how many solutions it wraps — see [Solution vs Plateau](#solution-vs-plateau).
- A Plateau is not necessarily deployed or used by the system as-is — it can exist purely as a reusable capability that other plateaus compose. Whether a given plateau is meant to stand alone is a fact about that plateau, stated explicitly via `standalone` (see `# Fields`), never left implied by its name or its position in a composition diagram.
- Composing a Plateau from several parent plateaus is a **union by default (AND)**: every parent's content is included, plus whatever this plateau's own `created_by` solutions add on top. A conflict — two parents disagreeing on the same file, rule, or structural element, or a parent disagreeing with this plateau's own additions — is never resolved silently: stop and ask the user, then record the resolution as a plateau-level ADR, exactly as [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions already does for solution-level conflicts]], generalized here to plateau-level ones.
- A Solution may build on top of **at most one** Plateau. If a second plateau seems to be needed, that is a sign the two plateaus should first be composed into one (via `parent_plateaus`), not that a solution should point at two.

# Solution vs Plateau
The test is not "how many solutions does this bundle wrap" — a Plateau can wrap exactly one. The test is **how this bundle will be referenced from now on**:

- Will something later want to point at this bundle **as a single named unit** — via `built_on_plateau` or `parent_plateaus` — without caring which individual solutions are inside it? → Package it as a **Plateau**, even if it is currently just one solution.
- Will consumers keep picking and choosing individual solutions out of this bundle, mixing them freely with others depending on the situation? → Leave it as loose **Solutions**. Do not force them into a plateau just because they happen to sit next to each other today — that only adds a layer nothing points at.

Concretely: ask whether the bundle delivers one coherent, nameable piece of *architectural* functionality (a validation stack, a persistence stack, a shared rules capability) that another plateau or solution would rather depend on as a whole than assemble itself from parts. If yes, it earns a plateau, regardless of size.

Example: `solution-domain-rules`, together with its own dedicated test-project setup, is worth its own plateau — not because that makes it "big enough", but because other services already want to depend on "the rules capability" as one thing. That the rules and their tests also happen to be portable to another language is a *consequence* of that coherence, not the test itself — a single-language, non-portable solution can just as well deserve its own plateau if the same "depended on as a unit" condition holds.

# Fields

## On a Plateau
| Field | Type | Meaning |
| --- | --- | --- |
| `parent_plateaus` | list of wikilinks | Every plateau this one is composed from. Empty when built from scratch. A list of one element expresses what the old singular `parent_plateau` used to mean (a delta on top of one base); a list of several expresses composition of independent capabilities into a larger one — both are the same mechanism at different sizes, not two different fields. |
| `created_by` | list of wikilinks | Solutions applied directly by this plateau, on top of whatever `parent_plateaus` already contribute — unchanged from [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]. |
| `standalone` | boolean | Whether this plateau is meant to be usable/deployable on its own, not only as an ingredient composed into a larger plateau. Always set explicitly. |

## On a Solution
| Field | Type | Meaning |
| --- | --- | --- |
| `depends_on` | list of wikilinks | Already defined by [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] — sibling solutions this one requires directly, at the same granularity (a specific mechanism needing another specific mechanism). Listed here only for contrast with `built_on_plateau`, not redefined. |
| `built_on_plateau` | single wikilink (optional) | The one plateau this solution assumes already exists and builds on top of — a coarser relationship than `depends_on`, pointing at an assembled capability instead of a sibling mechanism. A reviewer must be able to tell "needs this whole plateau" from "needs these specific solutions" at a glance, without cross-referencing every `depends_on` entry against every plateau's `created_by`. Leave empty for a solution meant to be usable before any plateau exists yet (typically the first, foundational solution a plateau is built from). |

# Rule

## MUST
- Give every plateau with a non-empty `parent_plateaus` the union of every parent's content by default.
  - Risk: silently picking one parent over another when they overlap loses content the author expected to still be there.
  - Fix: merge every parent's content; treat overlap as a conflict to resolve explicitly, never as a priority order to guess at.
- Stop and ask the user, then record a plateau-level ADR, the moment two parents — or a parent and this plateau's own `created_by` solutions — disagree on the same file, rule, or structural element.
  - Risk: silently picking a side hides a real design decision inside a diff no one reviewed as a decision, and the next person to compose the same parents differently re-introduces the same unresolved conflict.
  - Fix: follow [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions's existing conflict-ADR mechanism]], scoped to the composing plateau.
- Declare `built_on_plateau` on a solution instead of adding a `depends_on` entry for every individual solution the target plateau happens to contain.
  - Risk: re-declaring N individual solution edges hides the real shape of the dependency (this solution needs an entire plateau) and silently drifts out of sync the moment that plateau's own composition changes — this is the same failure mode `solution-plateau-hierarchy` exists to prevent one level up, at the solution-to-solution edge (see the real incident recorded in [[skills/common-workflow/architecture/example_architecture/groups/group-request-handling.skill/adr/depends-on-domain-modeling-as-group.md|group-request-handling's ADR]]).
  - Fix: point `built_on_plateau` at the plateau itself; let the plateau's own `parent_plateaus`/`created_by` stay the single source of truth for what that implies.
- State `standalone: true` or `standalone: false` on every plateau explicitly — never leave it to be inferred from the plateau's name or its position in a composition diagram.
  - Risk: a reader cannot tell "capability ingredient, not meant to run alone" from "finished, deployable profile" without this being written down, and may try to deploy an ingredient plateau on its own.
  - Fix: set the field on every plateau; a plateau composed purely to be composed further sets `standalone: false`.
- Give a solution at most one `built_on_plateau`.
  - Risk: a solution that appears to need two plateaus usually needs one plateau that composes both — allowing two edges here hides that composition should have happened one level up.
  - Fix: compose the two plateaus into one via `parent_plateaus` first, then point the solution at the composed plateau.

## SHOULD
- Prefer composing an existing plateau over re-deriving its solutions into a new one, when the same capability is needed again.
- Name every plateau with the `plateau-` prefix per [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]'s naming rule, even a small, single-solution plateau.

## MAY
- Give a plateau an empty `parent_plateaus` when it is the foundation every other plateau in the catalog is eventually composed from.

# Check list
- [ ] Every plateau declares `parent_plateaus` as a list (possibly empty) — no plateau uses the old singular `parent_plateau`.
- [ ] Every plateau declares `standalone: true` or `standalone: false` explicitly.
- [ ] A conflict between two parent plateaus, or between a parent and this plateau's own solutions, is recorded as a plateau-level ADR — never resolved silently.
- [ ] A solution that builds on a plateau uses `built_on_plateau`, not one `depends_on` entry per solution inside that plateau.
- [ ] No solution declares more than one `built_on_plateau`.
- [ ] A candidate plateau was evaluated against [Solution vs Plateau](#solution-vs-plateau) before being created as a solution instead.
