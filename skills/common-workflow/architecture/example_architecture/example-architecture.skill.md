---
name: example-architecture
description: Worked example of a four-level architecture-skill hierarchy — Plateau, Solution Group, Solution (mechanism or decision), ADR — built from scratch for a tiny .NET module, demonstrating how to split solution skills into groups instead of a flat catalog
whenToUse: when designing a new set of architecture solution skills for a stack/domain and deciding how to split them into files, or when introducing grouping into an existing flat solution catalog
tags:
  - skill/architecture/design
  - stack/dotnet
  - concern/architecture
  - concern/documentation
---

# Goal
- Show, on a small concrete example, how to split architecture-solution skills into a four-level hierarchy instead of one flat folder: **Plateau → Solution Group → Solution → ADR**.
- Make the two structural gaps a flat solution catalog has explicit and fix them: no recorded home for "why do these solutions cluster", and `depends_on` used for two different things — a real prerequisite, and "these solutions merely run in the same lifecycle".
- Give an agent a template it can copy: a group skill's frontmatter/section shape, and the rule for when a dependency belongs on the group versus on one solution inside it.

# Core Principle
- A **Solution** stays exactly what [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] already defines: one self-contained, reusable mechanism, with its own `Implementation/` files and its own ADRs. This example does not change that unit — it adds a layer above it.
- A **Solution Group** is a new, named concern family — the thing a solution's name-prefix used to imply informally (`solution-domain-*`, `solution-entity-*`) becomes a real skill file with its own Goal and its own `depends_on`.
- `depends_on` means "cannot exist without" — never "happens to run in the same request lifecycle". A solution declares `depends_on` on a *sibling inside its own group* freely, but a dependency that reaches into *another* group is declared **once, on the group**, not repeated on every solution that happens to touch that group. A solution may still declare a narrow cross-group `depends_on` on one specific sibling solution, but only when it has a concrete `Implementation/*.extend.md` target inside that solution — never because the two "are used together".
- A **Solution** is one of two kinds, named by a `kind:` frontmatter field:
  - `kind: mechanism` — produces real `creates`/`extends` targets; this is what every solution in the catalog looks like today.
  - `kind: decision` — produces no code of its own; it picks between sibling mechanism solutions (or between "keep local" and "apply a mechanism solution") and must still show, in its `Implementation/`, what each branch produces — per [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]'s existing rule for classification/decision skills.
- A **Plateau** is built exactly as [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] already defines, from the flat list of `Implementation/` files across all applied solutions — groups do not change plateau assembly, they only change how solutions are organized and how their dependencies are declared before assembly.

# Group skill structure
A group is a Human Dir skill: `group-{group-name}.skill/group-{group-name}.skill.md`.

| Frontmatter field | Meaning |
| --- | --- |
| `name` | `group-{group-name}` |
| `description` | The concern family this group owns, one sentence |
| `whenToUse` | When an agent should open this group before picking a solution inside it |
| `domain: skill` / `type: solution-group` | Marks the file as a group, not a solution |
| `tags` | Carries a `group/{group-name}` facet tag — a new, dynamic facet mirroring `solution/*` (see [facet-vocabulary.md](../../skill-design.skill/facet-vocabulary.md)); not yet registered repo-wide, shown here as the proposed pattern |
| `contains` | Wikilinks to every solution skill in the group |
| `depends_on` | Wikilinks to **other groups** this group cannot be applied without — never individual solutions in another group |
| `adr` | Group-level decisions: which group `depends_on` which, and why a candidate solution was placed in this group instead of another |

# How to build a group
1. Look at the solutions already planned or already written. If two or more solutions share the same *reason to exist* (the same concern, the same "why would an agent open this file") rather than merely the same technical mechanism, they are candidates for one group.
2. Name the group after the concern, not after a technology (`domain-modeling`, not `fluent-validation`).
3. Move every cross-group `depends_on` currently declared on individual solutions inside the group up to the group's own `depends_on`, unless that specific edge has a concrete `Implementation/*.extend.md` target — keep those narrow edges on the solution instead of the group.
4. Write one group-level ADR whenever this collapsing decision is not obvious, or whenever a solution's group placement was itself debatable.
5. Nothing about `Implementation/` or plateau assembly changes — [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] still reads solutions directly.

# Worked example
This folder implements one tiny module end to end, so every rule above has a concrete file behind it: a `Customer` entity with an `Email` value object, where the same "is this a valid email" condition starts duplicated in two places and gets centralized once that duplication is real.

See [example-architecture-overview.canvas](./example-architecture-overview.canvas) for a diagram of how the solutions below connect, built following [[skills/common-workflow/architecture/design/solution-dependency-canvas-update.skill/solution-dependency-canvas-update.skill.md|solution-dependency-canvas-update]]'s edge/layout conventions (prerequisite → dependent, left-to-right, transitive reduction) plus two additions that convention doesn't cover yet: the two group boxes, and the cyan edges showing every solution feeding into `plateau-example` — the moment the plateau itself appears.

- [[./groups/group-domain-modeling.skill/group-domain-modeling.skill.md|group-domain-modeling]] — owns the `Email` value object, the `Customer` entity invariant, and the decision of where a duplicated condition should live.
  - [[./solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] (`kind: mechanism`)
  - [[./solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]] (`kind: mechanism`)
  - [[./solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] (`kind: mechanism`)
  - [[./solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]] (`kind: decision`) — dispatches between the three mechanism solutions above
- [[./groups/group-request-handling.skill/group-request-handling.skill.md|group-request-handling]] — owns the command + handler + transport validation for changing a customer's email. `depends_on` `group-domain-modeling` **as a group** — see its own [[./groups/group-request-handling.skill/adr/depends-on-domain-modeling-as-group.md|ADR]] for why, including the real incident that motivated it.
  - [[./solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]] (`kind: mechanism`)
  - [[./solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] (`kind: mechanism`)
- [[./solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]] (`kind: mechanism`) — creates the two projects both groups build on. It belongs to neither group: it is the foundation every group's `depends_on` implicitly assumes, the same role `solution-sln-structure` plays in the real .NET catalog today.
- [[./plateau/example/plateau-example.skill.md|plateau-example]] — the plateau assembled from all six solutions above, exactly as `plateau-create-by-solutions` prescribes. Its `structure/` folder is the payoff: an agent asked to add a new value object, entity, or command handler to this module opens one file there and knows exactly what to write.

# Rule

## MUST
- Give a group `depends_on` only other groups or the shared foundation solution, never an individual solution that lives inside another group.
  - Risk: individual cross-group edges hide the real shape of the dependency (one group needs another as a whole) behind N unrelated-looking edges, and it becomes easy to add a wrong one by association instead of by necessity — this is exactly what happened to `solution-validation-behavior`'s stray `depends_on` on `solution-repository-integration` in the real catalog, added because the two "are used in the same handler", not because one requires the other.
  - Fix: declare the dependency once on the group; if one specific solution genuinely needs one specific sibling in another group, keep that edge on the solution only when it has a concrete `Implementation/*.extend.md` target, and say so in `# Boundaries`/`# Requirements`.
- Give a `kind: decision` solution an `Implementation/` folder showing the concrete result of every branch it chooses between, even though it creates no code of its own.
  - Risk: an agent reading the decision solution sees a rule with no shape and cannot tell what "centralize" or "keep local" actually produces.
  - Fix: add one file per branch under `Implementation/` (see [[./solutions/solution-condition-ownership.skill/Implementation/decision-table.md|solution-condition-ownership's decision table]] for the pattern).
- Record the group's own dependency decision as an ADR whenever it collapses more than one previously-separate solution-level edge.
  - Risk: the next person to touch the group re-adds a per-solution edge because the "why" of the collapse is not written anywhere.
  - Fix: follow [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md|adr-create]] and store the ADR in the group's own `adr/` folder.

# Check list
- [ ] Every solution in the folder declares `kind: mechanism` or `kind: decision` in its frontmatter.
- [ ] Every group's `depends_on` lists only other groups or the shared foundation solution — no individual solution from another group.
- [ ] Every cross-group solution-to-solution `depends_on` that does exist has a concrete `Implementation/*.extend.md` target justifying it.
- [ ] `solution-condition-ownership` has an `Implementation/` file showing both the "keep local" and "centralize" outcomes.
- [ ] `plateau-example` is assembled following [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] without any change to that process.
