---
name: variability-map-create
description: Define how to build and maintain a Variability Map — one table binding every Variation Point of a plateau/solution catalog to the solutions that realize it (Variants, Constraint, Realized by, Realization depends on, Migration)
whenToUse: when a plateau/solution catalog needs its variability made explicit as a table instead of tribal knowledge — when grouping a Feature Model's non-common features into Variation Points, or when a new optional/alternative solution is added to the catalog
updated: 20260906
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|One map per catalog, not per plateau]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|Constraint vs. ordering stays column-level, not a depends_on schema change]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map|The map binds VPs to solutions; the plateau↔VP view lives in plateau-map-create]]"
---

# Goal
Produce `{catalog}/variability-map.md` — the binding between the catalog's Variation Points and the solutions that realize them, with every column filled. Concretely:
- **Variation Point table** - One row per axis on which two teams building on this catalog could legitimately answer differently, with Variants, Constraint, Realization depends on, and Migration filled directly by this skill — so an agent deciding what a new plateau contains reads one table instead of reverse-engineering intent from `created_by`/`depends_on` lists.
- **Completed Realized by column** - The `Realized by` column filled by the [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] sub-step, which walks each VP, authors or selects its realizing solution(s), and classifies their intersections — the map is not finished until this runs.
- **Surfaced missing edges** - Every requirement found in prose or in the Feature Model but absent from a solution's `depends_on` raised as a fix proposal, not left as an unencoded assumption.

# Core Principle
- **Two teams, two answers** - A [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]] exists only where two teams building on this catalog could legitimately answer differently. If every path through the catalog includes a solution, it is shared core (see [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/program-families|Program Families]]) — it does not get a row.
- **The table is the artifact** - A plateau never re-describes its own variability separately from the map. [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]] always points at solutions that exist — this skill never re-authors solution content.
- **Runs after the Feature Model** - The catalog's Feature Model feeds this skill its candidate features.
- **Realized by is a sub-step, not a later stage** - Filling `Realized by` — authoring each VP's realizing solution(s) and classifying where two of them touch the same code element — is done by [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]], run to complete this map. Assembling plateaus from the finished map is a separate stage this skill does not describe.

# Where the map lives
One Variability Map per catalog, at `{catalog}/variability-map.md` — a sibling of the catalog's `plateau/` and `solutions/` folders (e.g. `skills/dotnet/architecture/v3/variability-map.md`). Not one per plateau: a plateau is one *point* in the combination space the map describes, not a separate space of its own — splitting the map per plateau would duplicate the same VP row into every plateau that happens to touch it. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|adr/one-map-per-catalog]].

# How to build a Variability Map
1. Identify {catalog} — the folder holding the plateau/solution tree (e.g. `skills/dotnet/architecture/v3/`).
2. Read `{catalog}/feature/feature-model.md` when it exists (the previous pipeline step): its non-common features are the primary candidate list. Merge it with every solution reachable through any plateau's `created_by` (directly or via `parent_plateaus`) across the whole catalog — the union is the candidate pool.
3. For each candidate (or tight group of candidates answering one question), apply the Core Principle's test: would two teams legitimately answer differently? Discard candidates that appear on every existing and every reasonable future path — they are core, not variability.
4. Decide each VP's Variant shape per [Alternatives share a VP, combinables split](#alternatives-share-a-vp-combinables-split); use [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/variability-map.template|templates/variability-map.template.md]]'s worked entity-kind row as the model for a categorical VP.
5. Fill **Constraint** from evidence, not invention (see [Constraints from evidence only](#constraints-from-evidence-only)). When prose states a requirement absent from the solution's `depends_on`, treat this as a defect: raise a proposal to add the missing `depends_on` entry to that solution, rather than only noting the gap in the table.
6. Fill **Realization depends on** using the three relations from [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|Orthogonal Variability Model]]: mandatory sub-feature, orthogonal VP, or cross-feature interaction — this is where a same-VP dependency that changes code *shape* (not just whether the VP is allowed) belongs, distinct from Constraint.
7. Fill **[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/migration|Migration]]**: `Yes` only for a VP whose answer is known to have changed on a service that already runs on this catalog (a binding-time question, not a design-time one); `No` otherwise, and `No` is the default — see the glossary for when a VP is design-time bound.
8. Fill **Realized by** by running [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] — with VP, Variants, and Constraint set, that skill walks each VP, authors or selects the solution skill(s) that realize it, and classifies where two solutions touch the same code element. The map is not finished until this runs. This skill never authors solution content itself; the column always holds wikilinks to real solution files.
9. Confirm every column is filled for every row. A downstream stage assembles plateaus from the finished map's Realized-by combinations; that stage is not part of this skill.

Use [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/variability-map.template|templates/variability-map.template.md]] for the full column set and worked structure. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/examples/example-dotnet-variability-map|examples/example-dotnet-variability-map.md]] for a walkthrough against a real catalog.

# Rule

## MUST

### A row only for a real decision
Create a VP row only for an axis where two teams could legitimately answer differently; never materialize a row for something every path through the catalog already includes.
- Risk: the map balloons with rows that carry no real decision, burying the axes that actually matter under ones that don't.
- Fix: apply the Core Principle's test before adding a row; if every existing plateau answers it the same way and no stated future need suggests otherwise, leave it as core.

### Realized by links, never copies
Point **Realized by** at existing solution skills via wikilink; never copy or re-summarize a solution's own content into the table.
- Risk: the table and the solution skill drift apart the moment either one is edited, and a reader cannot tell which is authoritative.
- Fix: link the solution skill file; keep the table itself to VP/Variant/Constraint bookkeeping.

### Realized by is filled by the delta-conflict-detection step
Do not populate **Realized by** ad hoc; run [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] once the other columns are set, and let it author/select each VP's solutions and classify their intersections.
- Risk: solutions chosen without the classifier miss constrained-delta conflicts, so the map links a combination that cannot actually coexist in one plateau.
- Fix: treat the `Realized by` step as a required sub-step — the map is incomplete until `delta-conflict-detection` has run over every VP row.

### Constraints from evidence only
Derive every **Constraint** entry from real evidence — an existing `depends_on`/`built_on_plateau` edge, a requirement already stated in a solution's own prose, or an owner-confirmed `Requires` edge in the catalog's Feature Model — never invent one.
- Risk: a fabricated constraint blocks a legitimate combination a team actually needs, or hides a real one a team needed protection from.
- Fix: check the actual solution files (and the Feature Model, when the catalog has one) before writing a Constraint; when prose states a requirement `depends_on` does not encode, flag and fix the solution instead of only noting the gap.

### depends_on stays a plain list
Keep `depends_on`'s shape unchanged on every solution skill (plain wikilink list, no per-entry annotation) — represent the Constraint-vs-Realization-depends-on distinction only in this table's own columns.
- Risk: annotating `depends_on` entries (e.g. adding a `reason:` field) changes a schema every stack's `solution-create` shares, rippling across every existing solution in the repository for a distinction only this table needs.
- Fix: keep `depends_on` a plain list everywhere; put the "why" — constraint vs. pure ordering — only in this table's Constraint / Realization depends on columns. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|adr/constraint-vs-ordering-columns]].

### No plateau view in the map
Do not derive a plateau↔VP matrix or plateau consistency checks inside `variability-map.md` — the map binds VPs to solutions and ends there. The plateau↔VP view is built by `plateau-map-create` in `{catalog}/plateau/plateau-repository.md`.
- Risk: a plateau-oriented section gives the map a second audience and a second trigger (plateau changes), and duplicates whatever the plateau stage maintains.
- Fix: keep the artifact to the VP↔solution binding; the decision and where the view went are recorded in [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map|adr/no-plateau-view-in-variability-map]].

### Follow the skill-design baseline
Follow [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
- Risk: this skill's rules cover the Variability Map's content, not the mechanics every skill must follow — skipping the shared baseline produces a technically-correct map workflow in a non-conforming skill file.
- Fix: apply `skill-design.skill.md` in addition to, never instead of, the rules above.

## SHOULD

### Alternatives share a VP, combinables split
Group solutions into one VP with several Variants when they are mutually-exclusive alternative answers to the same question (the entity-kind shape); keep them as separate boolean VPs when they are freely, independently combinable (an "Or" group in Feature-Model terms, not an "Alternative" group). Never pre-enumerate combinations of independent VPs as separate rows — each stays its own row regardless of how many other VPs it can combine with.
- Risk: enumerating combinations doubles the table every time a new independent answer appears, and the rows stop being questions a team answers.
- Fix: one row per question; a categorical VP only for genuinely mutually-exclusive answers (see the [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/feature-model|Feature model]] shapes).

### Reuse a near-duplicate VP first
Prefer reusing an existing VP's Variant set over introducing a near-duplicate VP when a new solution answers almost the same question an existing VP already covers.

## MAY

### Migration defaults to No
Leave **Migration** at `No` for a VP that has never yet needed to change after a service was already composed on this catalog.

# Check list
- [ ] Every VP row passed the "would two teams legitimately answer differently" test before being added.
- [ ] The candidate pool included the Feature Model's non-common features when the catalog has `{catalog}/feature/feature-model.md`.
- [ ] The **Realized by** column was filled by running `delta-conflict-detection`, not ad hoc; every entry is a wikilink to an existing solution skill (a draft-marked skeleton counts), not inlined content.
- [ ] Every **Constraint** entry is traceable to a real `depends_on`/`built_on_plateau` edge, a solution's own stated prose requirement, or an owner-confirmed Feature-Model `Requires` edge.
- [ ] No solution skill's `depends_on` field was changed in shape to carry a constraint/ordering annotation.
- [ ] The map contains no plateau↔VP derivation — that view belongs to `plateau-map-create`.
- [ ] All `hint`/`example` blocks removed from the final `variability-map.md` (none should have been copied from the template in the first place).
- [ ] Facet tags follow [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]]: `concern/architecture`, bare `stack`.
