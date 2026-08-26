---
name: solution-plateau-hierarchy
description: Defines the relationship between Solutions, Plateaus, and Plateau Components — when something qualifies as a Plateau instead of a Solution, how a Plateau composes other Plateaus, how a Solution builds on top of a Plateau, and where the optional, cross-cutting Plateau Component fits alongside both
whenToUse: when deciding whether a new architectural unit should be a solution, a plateau, or a plateau component, when a plateau needs to be composed from other plateaus instead of assembled only from solutions, or when a solution needs to declare that it builds on top of an existing plateau instead of on individual solutions inside it
tags:
  - skill/architecture/design
  - stack
  - concern/architecture
---

# Goal
- Give every architectural unit an unambiguous place in one hierarchy — Solution, Plateau, or Plateau Component — so an agent building a new one knows immediately which shape to use.
- Let a Plateau be composed from other Plateaus, not only assembled from a flat list of Solutions, so a genuinely reusable capability (a validation stack, a persistence stack) is captured once and reused by every larger Plateau that needs it, instead of being re-declared as a `depends_on` on each solution inside it separately.
- Let a Solution declare that it builds on a Plateau as a whole, distinctly from `depends_on` on sibling solutions, for the same reason.
- Mark where the Plateau Component ([[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]) sits relative to Solution and Plateau — this skill only places it in the hierarchy and contrasts its fields; its own build rules and the full Solution vs Plateau vs Component test live in that skill.

# Core Principle
- A **Solution** ([[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]) is the atomic, reusable mechanism — it may `depends_on` other solutions, but never composes a plateau.
- A **Plateau** ([[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]) is a concretely-functional, agent-facing unit assembled from solutions and/or other plateaus. Whether something qualifies as a plateau is not a question of how many solutions it wraps — see [Solution vs Plateau](#solution-vs-plateau).
- A Plateau is not necessarily deployed or used by the system as-is — it can exist purely as a reusable capability that other plateaus compose. Whether a given plateau is meant to stand alone is a fact about that plateau, stated explicitly via `standalone` (see `# Fields`), never left implied by its name or its position in a composition diagram.
- Composing a Plateau from several parent plateaus is a **union by default (AND)**: every parent's content is included, plus whatever this plateau's own `created_by` solutions add on top. A conflict — two parents disagreeing on the same file, rule, or structural element, or a parent disagreeing with this plateau's own additions — is never resolved silently: stop and ask the user, then record the resolution as a plateau-level ADR, exactly as [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions already does for solution-level conflicts]], generalized here to plateau-level ones.
- A Solution may build on top of **at most one** Plateau. If a second plateau seems to be needed, that is a sign the two plateaus should first be composed into one (via `parent_plateaus`), not that a solution should point at two.
- A **Plateau Component** ([[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]) is the third unit in this hierarchy, but it does not compose or get composed the way Solution and Plateau do: it has no `parent_plateaus`/`created_by` of its own, and it is never listed in a Plateau's `created_by` — a Plateau's identity is exactly what its `parent_plateaus`/`created_by` say it is, and a Component is deliberately excluded from that so the same Plateau stays usable with or without it. See [Where Plateau Component fits](#where-plateau-component-fits).

# Solution vs Plateau
The test is not "how many solutions does this bundle wrap" — a Plateau can wrap exactly one. The test is **how this bundle will be referenced from now on**:

- Will something later want to point at this bundle **as a single named unit** — via `built_on_plateau` or `parent_plateaus` — without caring which individual solutions are inside it? → Package it as a **Plateau**, even if it is currently just one solution.
- Will consumers keep picking and choosing individual solutions out of this bundle, mixing them freely with others depending on the situation? → Leave it as loose **Solutions**. Do not force them into a plateau just because they happen to sit next to each other today — that only adds a layer nothing points at.

Concretely: ask whether the bundle delivers one coherent, nameable piece of *architectural* functionality (a validation stack, a persistence stack, a shared rules capability) that another plateau or solution would rather depend on as a whole than assemble itself from parts. If yes, it earns a plateau, regardless of size.

Example: `solution-domain-rules`, together with its own dedicated test-project setup, is worth its own plateau — not because that makes it "big enough", but because other services already want to depend on "the rules capability" as one thing. That the rules and their tests also happen to be portable to another language is a *consequence* of that coherence, not the test itself — a single-language, non-portable solution can just as well deserve its own plateau if the same "depended on as a unit" condition holds.

# Where Plateau Component fits
Solution and Plateau both answer "how does this get *composed*". Plateau Component answers a different question — "what optionally *attaches* afterward, without changing what got composed". Run the Solution vs Plateau test above first; only when the candidate turns out to be self-contained, optional, and touches nothing but the composition root, re-check it against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create's Solution vs Plateau vs Component test]] before building it as a Solution or folding it into a Plateau's `created_by`.

The structural difference this hierarchy cares about: a Plateau's identity is exactly its `parent_plateaus` + `created_by` — remove an entry and it is a different plateau. A Component is deliberately kept outside that identity: it has no `parent_plateaus`/`created_by` of its own, is never added to a Plateau's `created_by`, and its own `built_on_plateau` doesn't mean "composed into this plateau" the way a Solution's does — it means "usable on this plateau and on everything composed on top of it in the same `parent_plateaus` lineage", since a Component never depends on which Solutions a given plateau happens to contain.

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

## On a Plateau Component
| Field | Type | Meaning |
| --- | --- | --- |
| `built_on_plateau` | single wikilink (optional) | Same field name as on a Solution, different reach: states the minimum plateau baseline the component needs, but the component stays valid on that plateau **and** on every plateau composed on top of it (deeper in the same `parent_plateaus` lineage) — it is never "composed into" one specific plateau's own definition. Full field list and build rules in [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]. |
| `parent_plateaus` / `created_by` | — | Never set on a Component — see [Where Plateau Component fits](#where-plateau-component-fits). |

# Rule

## MUST
- Give every plateau with a non-empty `parent_plateaus` the union of every parent's content by default.
  - Risk: silently picking one parent over another when they overlap loses content the author expected to still be there.
  - Fix: merge every parent's content; treat overlap as a conflict to resolve explicitly, never as a priority order to guess at.
- Stop and ask the user, then record a plateau-level ADR, the moment two parents — or a parent and this plateau's own `created_by` solutions — disagree on the same file, rule, or structural element.
  - Risk: silently picking a side hides a real design decision inside a diff no one reviewed as a decision, and the next person to compose the same parents differently re-introduces the same unresolved conflict.
  - Fix: follow [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions's existing conflict-ADR mechanism]], scoped to the composing plateau.
- Declare `built_on_plateau` on a solution instead of adding a `depends_on` entry for every individual solution the target plateau happens to contain.
  - Risk: re-declaring N individual solution edges hides the real shape of the dependency (this solution needs an entire plateau) and silently drifts out of sync the moment that plateau's own composition changes — this is the same failure mode `solution-plateau-hierarchy` exists to prevent one level up, at the solution-to-solution edge (this repository's real .NET catalog once carried exactly this kind of stale edge: `solution-validation-behavior` depended on `solution-repository-integration` for a reason that was never actually true, and it went unnoticed until an explicit review).
  - Fix: point `built_on_plateau` at the plateau itself; let the plateau's own `parent_plateaus`/`created_by` stay the single source of truth for what that implies.
- State `standalone: true` or `standalone: false` on every plateau explicitly — never leave it to be inferred from the plateau's name or its position in a composition diagram.
  - Risk: a reader cannot tell "capability ingredient, not meant to run alone" from "finished, deployable profile" without this being written down, and may try to deploy an ingredient plateau on its own.
  - Fix: set the field on every plateau; a plateau composed purely to be composed further sets `standalone: false`.
- Give a solution at most one `built_on_plateau`.
  - Risk: a solution that appears to need two plateaus usually needs one plateau that composes both — allowing two edges here hides that composition should have happened one level up.
  - Fix: compose the two plateaus into one via `parent_plateaus` first, then point the solution at the composed plateau.
- Rule out a Plateau Component before defaulting a new unit to Solution or Plateau, and never add a Component to a Plateau's `parent_plateaus`/`created_by`.
  - Risk: a self-contained, optional, composition-root-only capability (e.g. logging) gets baked into a Plateau's own identity, forcing every user of that plateau to take it and forcing a second plateau variant to exist for "without it".
  - Fix: check the candidate against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create's test]]; build it there and attach it separately if it applies.

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
- [ ] A self-contained, optional, composition-root-only candidate was evaluated against `plateau-component-create`'s Solution vs Plateau vs Component test before being built as a Solution or added to a Plateau's `created_by`.
- [ ] No Plateau declares a Plateau Component in `parent_plateaus` or `created_by`.
