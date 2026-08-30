---
name: variability-map-create
description: Define how to build and maintain a Variability Map — one table listing every Variation Point across a plateau/solution catalog, its Variants, Constraints, and Realized-by solutions — and how the catalog's Plateau Map (its named plateaus) is derived from it as specific, checkable points in the Variation-Point combination space
whenToUse: when a plateau/solution catalog needs its variability made explicit as a table instead of tribal knowledge — before creating a new plateau whose combination of solutions is not obviously already covered, when a new optional/alternative solution is added to the catalog, or when reviewing whether the existing named plateaus still cover every legitimate combination teams actually choose between
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|One map per catalog, not per plateau]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|Constraint vs. ordering stays column-level, not a depends_on schema change]]"
---

# Goal
- Give every axis on which a plateau/solution catalog legitimately varies a single, explicit row — a [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]] — so an agent deciding what a new plateau should contain reads one table instead of reverse-engineering intent from `created_by`/`depends_on` lists.
- Make the catalog's existing named plateaus a *derived, checkable consequence* of that table (its [Plateau Map](#plateau-map-derivation)) instead of an ad-hoc, hand-maintained tree that can silently drift from what the table says is actually allowed.
- Surface a real Constraint the moment it exists in prose or code but is missing from a solution's own `depends_on`, instead of letting it stay an unencoded assumption.

# Core Principle
- A [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]] exists only where two teams building on this catalog could legitimately answer differently. If every path through the catalog includes a solution, it is shared core (see [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/program-families|Program Families]]) — it does not get a row.
- The table is the single artifact; a plateau never re-describes its own variability separately from it. [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]] always points at solutions that already exist via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] — this skill never re-authors solution content.
- This skill runs *before* [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] in the pipeline: a Variability Map row's Realized-by combination becomes that skill's `{solutions}` input. This skill never builds a plateau itself, and never edits `plateau-create-by-solutions`, `plateau-update-by-solutions`, or `solution-plateau-hierarchy` — it only reads and references them.
- Once conflicts are found between two Realized-by solutions sharing an element, resolving them is out of scope here — see [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill|delta-conflict-detection]], which runs next and consumes this table's Realized-by column as its own input.

# Where the map lives
One Variability Map per catalog, at `{catalog}/variability-map.md` — a sibling of the catalog's `plateau/` and `solutions/` folders (e.g. `skills/dotnet/architecture/v3/variability-map.md`). Not one per plateau: a plateau is one *point* in the combination space the map describes, not a separate space of its own — splitting the map per plateau would duplicate the same VP row into every plateau that happens to touch it. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|adr/one-map-per-catalog]].

# How to build a Variability Map
1. Identify {catalog} — the folder holding the plateau/solution tree (e.g. `skills/dotnet/architecture/v3/`).
2. Enumerate every solution reachable through any plateau's `created_by` (directly or via `parent_plateaus`) across the whole catalog — this is the candidate pool.
3. For each solution (or tight group of solutions answering one question), apply the Core Principle's test: would two teams legitimately answer differently? Discard candidates that appear on every existing and every reasonable future path — they are core, not variability.
4. Decide the VP's Variant shape:
   - **Multiple mutually-exclusive Variants, one VP** — when the candidates are alternative answers to one question (a categorical choice). See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/feature-model|Feature model]] for the general shape, and use [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/variability-map.template|templates/variability-map.template.md]]'s worked entity-kind row as the model.
   - **Separate boolean VPs (Yes/No)** — when the candidates are freely, independently combinable (an "Or" group in Feature-Model terms, not an "Alternative" group). Never pre-enumerate their combinations as separate rows — each stays its own row regardless of how many other VPs it can combine with.
5. Fill **Constraint** from evidence, not invention: an existing `depends_on`/`built_on_plateau` edge between the VP's solutions and another VP's solutions, or a requirement already stated in a solution's own prose (`description`/`whenToUse`/Requirements section). When prose states a requirement absent from the solution's `depends_on`, treat this as a defect: propose adding the missing `depends_on` entry to that solution (recording the fix as a plateau-level ADR per [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions]]) rather than only noting the gap in the table.
6. Fill **Realized by** with wikilinks to the actual solution skill(s) — never inline a copy of what the solution does.
7. Fill **Realization depends on** using the three relations from [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|Orthogonal Variability Model]]: mandatory sub-feature, orthogonal VP, or cross-feature interaction — this is where a same-VP dependency that changes code *shape* (not just whether the VP is allowed) belongs, distinct from Constraint.
8. Fill **Migration**: `Yes` only for a VP whose answer is known to change after a service built on this catalog already exists (a binding-time question, not a design-time one); `No` otherwise.
9. Write the [Plateau Map derivation](#plateau-map-derivation) section.
10. Hand the finished Realized-by combinations to [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] (new plateau) or [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md|plateau-update-by-solutions]] (existing plateau) as their `{solutions}`/`{parent_plateaus}` input.

# Plateau Map derivation
A dedicated section inside `variability-map.md`, one row per existing plateau in the catalog, stating the exact VP-answer combination that plateau fixes:

| Plateau | VP answers fixed |
| --- | --- |
| `plateau-{name}` | `VP1=Yes, VP2=No, ...` |

Treat every plateau as a claim that this combination is both internally consistent (violates no stated Constraint) and worth naming. If a plateau's combination *does* violate a stated Constraint, that is a defect in the catalog, not a special case to silently accept — raise it as a plateau-level ADR and fix either the plateau or the Constraint.

Use [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/variability-map.template|templates/variability-map.template.md]] for the full column set and worked structure. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/examples/example-dotnet-variability-map|examples/example-dotnet-variability-map.md]] for a walkthrough against a real catalog.

# Rule

## MUST
- Create a VP row only for an axis where two teams could legitimately answer differently; never materialize a row for something every path through the catalog already includes.
  - Risk: the map balloons with rows that carry no real decision, burying the axes that actually matter under ones that don't.
  - Fix: apply the Core Principle's test before adding a row; if every existing plateau answers it the same way and no stated future need suggests otherwise, leave it as core.
- Point **Realized by** at existing solution skills via wikilink; never copy or re-summarize a solution's own content into the table.
  - Risk: the table and the solution skill drift apart the moment either one is edited, and a reader cannot tell which is authoritative.
  - Fix: link the solution skill file; keep the table itself to VP/Variant/Constraint bookkeeping.
- Derive every **Constraint** entry from real evidence — an existing `depends_on`/`built_on_plateau` edge, or a requirement already stated in a solution's own prose — never invent one.
  - Risk: a fabricated constraint blocks a legitimate combination a team actually needs, or hides a real one a team needed protection from.
  - Fix: check the actual solution files before writing a Constraint; when prose states a requirement `depends_on` does not encode, flag and fix the solution instead of only noting the gap.
- Keep `depends_on`'s shape unchanged on every solution skill (plain wikilink list, no per-entry annotation) — represent the Constraint-vs-Realization-depends-on distinction only in this table's own columns.
  - Risk: annotating `depends_on` entries (e.g. adding a `reason:` field) changes a schema every stack's `solution-create` shares, rippling across every existing solution in the repository for a distinction only this table needs.
  - Fix: keep `depends_on` a plain list everywhere; put the "why" — constraint vs. pure ordering — only in this table's Constraint / Realization depends on columns. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|adr/constraint-vs-ordering-columns]].
- Write the [Plateau Map derivation](#plateau-map-derivation) section for every plateau already in the catalog, and treat a plateau whose fixed combination violates a stated Constraint as a defect requiring a plateau-level ADR, never a silent exception.
  - Risk: an inconsistent plateau ships unnoticed because nothing ever checked it against the table meant to make combinations explicit.
  - Fix: derive every plateau's row directly from its actual `created_by`/`parent_plateaus`, and cross-check it against every Constraint row before considering the map finished.
- Re-check and update the Variability Map whenever [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md|plateau-update-by-solutions]] changes a plateau's `created_by` (solution added, updated, or removed).
  - Risk: the map silently stops matching the catalog it describes, and the next reader trusts a stale table.
  - Fix: after any `created_by` change, re-derive the affected plateau's row in [Plateau Map derivation](#plateau-map-derivation) and check whether any VP/Realized-by/Constraint entry needs updating too.
- Follow [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.

## SHOULD
- Group solutions into one VP with several Variants when they are mutually-exclusive alternative answers to the same question (the entity-kind shape); keep them as separate boolean VPs when they are freely, independently combinable.
- Prefer reusing an existing VP's Variant set over introducing a near-duplicate VP when a new solution answers almost the same question an existing VP already covers.

## MAY
- Leave **Migration** at `No` for a VP that has never yet needed to change after a service was already composed on this catalog.

# Check list
- [ ] Every VP row passed the "would two teams legitimately answer differently" test before being added.
- [ ] Every **Realized by** entry is a wikilink to an existing solution skill, not inlined content.
- [ ] Every **Constraint** entry is traceable to a real `depends_on`/`built_on_plateau` edge or a solution's own stated prose requirement.
- [ ] No solution skill's `depends_on` field was changed in shape to carry a constraint/ordering annotation.
- [ ] [Plateau Map derivation](#plateau-map-derivation) lists every plateau currently in the catalog with its fixed VP-answer combination.
- [ ] Every plateau's fixed combination was checked against every stated Constraint; any violation found was raised as a plateau-level ADR, not silently accepted.
- [ ] The map was re-derived after the most recent `plateau-update-by-solutions` change, if any.
- [ ] All `hint`/`example` blocks removed from the final `variability-map.md` (none should have been copied from the template in the first place).
- [ ] Facet tags follow [[skills/common-workflow/skill-design.skill/facet-vocabulary.md|facet-vocabulary]]: `concern/architecture`, bare `stack`.
