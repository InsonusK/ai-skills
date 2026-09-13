---
name: solution-plateau-hierarchy
description: Defines the relationship between Solutions, Plateaus, and Plateau Components — when something qualifies as a Plateau instead of a Solution, how a Plateau composes other Plateaus, how a Solution builds on top of a Plateau, and where the optional, cross-cutting Plateau Component fits alongside both
whenToUse: when deciding whether a new architectural unit should be a solution, a plateau, or a plateau component, when a plateau needs to be composed from other plateaus instead of assembled only from solutions, or when a solution needs to declare that it builds on top of an existing plateau instead of on individual solutions inside it
updated: 20260909
tags:
  - skill/architecture/design
  - stack
  - concern/architecture
---

# Goal
- Every architectural unit placed unambiguously in one hierarchy — Solution, Plateau, or Plateau Component.
- A Plateau composed from other Plateaus via `parent_plateaus`, not re-declared as a `depends_on` on each solution inside it.
- A Solution's dependency on a whole Plateau declared via `built_on_plateau`, distinct from `depends_on` on sibling solutions.
- The Plateau Component placed in the hierarchy relative to Solution and Plateau, with its fields contrasted — its own build rules live in [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]].

# Core Principle
- **Solution is the atomic mechanism** - A Solution ([[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]) is the atomic, reusable mechanism; it may `depends_on` other solutions but never composes a plateau.
- **Plateau is an assembled, agent-facing unit** - A Plateau ([[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]) is a concretely-functional unit assembled from solutions and/or other plateaus; whether something qualifies is about how it will be referenced, not how many solutions it wraps — see [Solution vs Plateau](#solution-vs-plateau).
- **standalone is explicit** - Whether a Plateau is meant to stand alone or to exist only as an ingredient composed into a larger plateau is stated via `standalone`, never implied by its name or its position in a composition diagram.
- **Composition is union by default** - Composing a Plateau from several parents includes every parent's content plus this plateau's own `created_by` delta (AND); a conflict between two parents, or a parent and this plateau's additions, stops for a user decision recorded as a plateau-level ADR.
- **A Solution builds on at most one Plateau** - If a second plateau seems needed, the two plateaus should first be composed into one via `parent_plateaus`, not pointed at from a solution twice.
- **A Component sits outside composition identity** - A Plateau Component ([[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]) has no `parent_plateaus`/`created_by` and is never listed in a Plateau's `created_by`, so the same Plateau stays usable with or without it — see [Where Plateau Component fits](#where-plateau-component-fits).

# Workflow
Placing a new architectural unit:
1. Run the [Solution vs Plateau](#solution-vs-plateau) test: decide whether something will later point at this bundle as one named unit (Plateau) or keep picking individual solutions out of it (loose Solutions).
2. When the candidate turns out to be self-contained, optional, and to touch nothing but the composition root, re-check it against [Where Plateau Component fits](#where-plateau-component-fits) and [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create's test]] before building it as a Solution or folding it into a Plateau's `created_by`.
3. Set the unit's frontmatter fields per [Fields](#fields).

## Solution vs Plateau
The test is not "how many solutions does this bundle wrap" — a Plateau can wrap exactly one. The test is **how this bundle will be referenced from now on**:

- Will something later point at this bundle **as a single named unit** — via `built_on_plateau` or `parent_plateaus` — without caring which individual solutions are inside it? → Package it as a **Plateau**, even if it is currently just one solution.
- Will consumers keep picking and choosing individual solutions out of this bundle, mixing them freely with others? → Leave it as loose **Solutions**. Do not force them into a plateau just because they sit next to each other today — that only adds a layer nothing points at.

Concretely: ask whether the bundle delivers one coherent, nameable piece of *architectural* functionality (a validation stack, a persistence stack, a shared rules capability) that another plateau or solution would rather depend on as a whole than assemble itself from parts. If yes, it earns a plateau, regardless of size. That the capability also happens to be portable to another language is a *consequence* of that coherence, not the test — a single-language solution can deserve its own plateau if the same "depended on as a unit" condition holds.

## Where Plateau Component fits
Solution and Plateau both answer "how does this get *composed*". A Plateau Component answers a different question — "what optionally *attaches* afterward, without changing what got composed". Run the Solution vs Plateau test first; only when the candidate turns out to be self-contained, optional, and composition-root-only, re-check it against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create's Solution vs Plateau vs Component test]].

The structural difference this hierarchy cares about: a Plateau's identity is exactly its `parent_plateaus` + `created_by` — remove an entry and it is a different plateau. A Component is kept outside that identity: it has no `parent_plateaus`/`created_by`, is never added to a Plateau's `created_by`, and its own `built_on_plateau` means "usable on this plateau and everything composed on top of it in the same `parent_plateaus` lineage", not "composed into this plateau".

## Fields

On a Plateau:

| Field | Type | Meaning |
| --- | --- | --- |
| `parent_plateaus` | list of wikilinks | Every plateau this one is composed from. Empty when built from scratch. A one-element list expresses what the old singular `parent_plateau` meant (a delta on one base); several elements express composition of independent capabilities — the same mechanism at different sizes. |
| `created_by` | list of wikilinks | Solutions applied directly by this plateau, on top of whatever `parent_plateaus` contribute — unchanged from [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]. |
| `standalone` | boolean | Whether this plateau is meant to be usable/deployable on its own, not only as an ingredient. Always set explicitly. |

On a Solution:

| Field | Type | Meaning |
| --- | --- | --- |
| `depends_on` | list of wikilinks | Sibling solutions this one requires directly, at the same granularity — defined by [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]], listed here only for contrast. |
| `built_on_plateau` | single wikilink (optional) | The one plateau this solution assumes already exists and builds on — a coarser relationship than `depends_on`, pointing at an assembled capability instead of a sibling mechanism. Empty for a solution meant to be usable before any plateau exists (typically the first, foundational solution a plateau is built from). |

On a Plateau Component:

| Field | Type | Meaning |
| --- | --- | --- |
| `built_on_plateau` | single wikilink (optional) | Same field name as on a Solution, different reach: the minimum plateau baseline the component needs; the component stays valid on that plateau **and** on every plateau composed on top of it, and is never "composed into" one plateau's definition. Full field list and build rules in [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]. |
| `parent_plateaus` / `created_by` | — | Never set on a Component — see [Where Plateau Component fits](#where-plateau-component-fits). |

# Rule

## MUST

### Union by default for parent_plateaus
Give every plateau with a non-empty `parent_plateaus` the union of every parent's content by default.
- Risk: silently picking one parent over another when they overlap loses content the author expected to still be there.
- Fix: merge every parent's content; treat overlap as a conflict to resolve explicitly, never as a priority order to guess.

### Stop and ADR on a composition conflict
Stop and ask the user, then record a plateau-level ADR, the moment two parents — or a parent and this plateau's own `created_by` solutions — disagree on the same file, rule, or structural element.
- Risk: silently picking a side hides a real design decision inside an unreviewed diff, and the next person composing the same parents re-introduces the conflict.
- Fix: follow [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions's conflict-ADR mechanism]], scoped to the composing plateau.

### Declare a whole-plateau dependency via built_on_plateau
Declare `built_on_plateau` on a solution instead of adding a `depends_on` entry for every individual solution the target plateau happens to contain.
- Risk: re-declaring N individual solution edges hides that the solution needs an entire plateau and drifts out of sync the moment that plateau's composition changes. (`solution-validation-behavior` carried a false `depends_on` on `solution-repository-integration` this way.)
- Fix: point `built_on_plateau` at the plateau itself; let the plateau's own `parent_plateaus`/`created_by` be the single source of truth for what that implies.

### State standalone explicitly
State `standalone: true` or `standalone: false` on every plateau explicitly — never leave it to be inferred from the plateau's name or diagram position.
- Risk: a reader cannot tell "capability ingredient, not meant to run alone" from "finished, deployable profile" and may deploy an ingredient plateau on its own.
- Fix: set the field on every plateau; a plateau composed purely to be composed further sets `standalone: false`.

### At most one built_on_plateau per solution
Give a solution at most one `built_on_plateau`.
- Risk: a solution that appears to need two plateaus usually needs one plateau composing both — allowing two edges hides that composition should have happened one level up.
- Fix: compose the two plateaus into one via `parent_plateaus` first, then point the solution at the composed plateau.

### Rule out a Plateau Component first
Rule out a Plateau Component before defaulting a new unit to Solution or Plateau, and never add a Component to a Plateau's `parent_plateaus`/`created_by`.
- Risk: a self-contained, optional, composition-root-only capability (e.g. logging) baked into a Plateau's identity forces every user of that plateau to take it and forces a second "without it" variant.
- Fix: check the candidate against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create's test]]; build it there and attach it separately if it applies.

## SHOULD

### Compose an existing plateau over re-deriving it
Prefer composing an existing plateau over re-deriving its solutions into a new one when the same capability is needed again.

### Prefix every plateau name with plateau-
Name every plateau with the `plateau-` prefix per [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]]'s naming rule, even a small single-solution plateau.

## MAY

### Empty parent_plateaus for the foundation
Give a plateau an empty `parent_plateaus` when it is the foundation every other plateau in the catalog is eventually composed from.

# Check list
- [ ] Every plateau declares `parent_plateaus` as a list (possibly empty) — no plateau uses the old singular `parent_plateau`.
- [ ] Every plateau declares `standalone: true` or `standalone: false` explicitly.
- [ ] A conflict between two parent plateaus, or between a parent and this plateau's own solutions, is recorded as a plateau-level ADR — never resolved silently.
- [ ] A solution that builds on a plateau uses `built_on_plateau`, not one `depends_on` entry per solution inside that plateau.
- [ ] No solution declares more than one `built_on_plateau`.
- [ ] A candidate plateau was evaluated against [Solution vs Plateau](#solution-vs-plateau) before being created as a solution instead.
- [ ] A self-contained, optional, composition-root-only candidate was evaluated against `plateau-component-create`'s Solution vs Plateau vs Component test before being built as a Solution or added to a Plateau's `created_by`.
- [ ] No Plateau declares a Plateau Component in `parent_plateaus` or `created_by`.
