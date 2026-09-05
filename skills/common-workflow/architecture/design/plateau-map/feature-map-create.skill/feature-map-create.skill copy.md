---
name: feature-map-create
description: Define how to build a Feature Model — a Program Family's common/mandatory baseline versus its variable feature set — as a FODA-style diagram (every edge typed) plus a Name/Description/IsCommon table, grounded in a concrete baseline project structure rather than in an existing catalog's current shape
whenToUse: when a plateau/solution catalog (existing or brand new) needs its Feature Model made explicit before any plateau is named or any Variability Map is built, or when reviewing whether a candidate capability belongs in the common baseline or in variability
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
---

# Goal
- **From vibes to reviewable artifact** - Turn a fuzzy sense of "what varies" into an explicit, reviewable artifact: a concrete baseline project structure, a FODA-style diagram with every relation typed, and a table with one row per feature stating whether it is common.
- **Baseline beats precedent** - Make the common/variable split defensible against a concrete, written-out baseline — never against "no existing plateau currently skips it," which proves nothing about whether skipping it is legitimate.
- **Feed variability-map-create** - Hand off a clean input to the next pipeline step: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] consumes this model's non-common features to build Variation Points and, from there, plateaus.

# Core Principle
- **Test candidates against real names** - A feature earns a row only once tested against a concrete baseline structure written out in `feature-model.md` itself (real folder/project names) — not against vibes, and not against what an existing catalog's plateaus currently happen to share.
- **Node and row correspond exactly** - The diagram and the table are two views of one model: every diagram node (except the root) has exactly one table row, and every row appears in the diagram.
- **No single unreviewed pass** - Build this in small, reviewed increments with whoever owns the Program Family — judgment calls made in one unreviewed pass surface only once someone looks closely.
- **Variation Points are downstream** - This skill's scope ends at the common/variable split and the diagram/table that express it. Grouping variable features into Variation Points, mapping `Realized by`, and resolving conflicts belong to [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] and [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]].

# Where the model lives
`{output}/feature/` — a sibling of the catalog's `plateau/`/`solutions/` folders (e.g. `skills/dotnet/architecture/v3.1/feature/`):
```
feature/
  feature-model.md
  diagrams/
    feature-diagram.mmd
```

# How to build a Feature Model
1. Identify {output} — the catalog root (existing or new).
2. **Write out the concrete baseline structure first** — the literal project/folder layout of a family member with nothing but common features (real names: `App.Host`, `{Module}.Application`, `Shared`, `BuildingBlocks`, ...). Every later step tests candidates against it. Derive the baseline from first principles — what the simplest legitimate family member needs to function at all — then check an existing catalog against it, never the other way round.
3. Enumerate candidate features from every source: an existing catalog's solutions, and fresh requirements not yet built anywhere. Mark aspirational candidates (no `Realized by` target exists yet) — the model describes the intended Program Family, not only what is already implemented.
4. Test each candidate against the baseline: requires anything beyond the baseline as written → variable; the baseline cannot function at all without it → common. Two traps:
   - Do not default a fixed-seeming capability (exception handling, logging, build/test gates) to "infrastructure, not a feature" without asking the owner — they may want it tracked as a first-class common feature expected to grow (a minimal console logger today, file-backed later).
   - Before modeling two candidates as separate features, verify they are not the same mechanism under two names (a "Command" and a "Query" on the identical dispatch mechanism are one feature until something *technical*, not just semantic, differs).
5. Decide each feature's shape:
   - **Leaf** — no further decomposition.
   - **Group with further features** — bundled children nest in the parent's block; independently selectable children sit outside, connected by a labeled edge (see **Bundled children nest, optional children don't** under `# Rule`).
6. Identify cross-tree constraints (a requirement between two features not connected by a parent-child edge). Verify each against an existing solution/pattern when possible; flag reasoning-only ones as unconfirmed (see **Reasoning-only constraints stay provisional** under `# Rule`).
7. Build `diagrams/feature-diagram.mmd` per [# The diagram](#the-diagram), starting from [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/feature-diagram.template.mmd|templates/feature-diagram.template.mmd]].
8. Build `feature-model.md` from [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/feature-model.template.md|templates/feature-model.template.md]]: the baseline structure (step 2), the root/product explanation, the `@import`ed diagram, the AND/OR logic of any parallel constraint edges, the Features table, a note on anything deliberately excluded from the table, any flagged/unconfirmed constraints, and the `Out of scope` section. Remove every `hint` block and the template's "How Apply this template" section before saving.
9. Confirm each materially new or changed part (a new feature, a rename, a constraint, a common/variable verdict) with the family's owner before building further on it.

# The diagram
Follow [[skills/common-workflow/mermaid-diagram.skill.md|mermaid-diagram]]: a diagram past ~5 elements is a separate `.mmd` file under `diagrams/`, embedded via `@import "./diagrams/feature-diagram.mmd" {as="mermaid"}` — never a plain fenced ```mermaid block at this size.

- **Root**: the family's product, named explicitly (e.g. `Module`); never a variability question, never a row in the Features table.
- **Common block**: the root plus every `Mandatory` feature in one visually distinct `subgraph Common[...]`; top-level variable features connect with edges *from the block itself* (`Common -->|Optional| X`).
- **Matrix layout**: once a block holds more than ~4 members, arrange them as rows — nested subgraphs with `direction LR` per row, `direction TB` on the outer block, row borders hidden (`style rowId fill:none,stroke:none`). Visible borders only for real sub-feature groups (e.g. `TestConformance`), never for layout-only rows.
- **Edge labels — every edge, from this closed list**: `Mandatory`, `Optional`, `Optional (at least one)` (the parent's selection requires one or more children — this repository's name for FODA's "Or" group; use this exact phrase), `Alternative (group name)` (reserved: exactly one — not yet used in any model, keep available), `Requires` (dotted, cross-tree, not a parent-child edge).
- **Parallel `Requires` edges into one target**: state the AND/OR logic in `feature-model.md` prose — edge labels cannot express it.

# Rule

## MUST
- **Baseline before commonality verdicts** - Write out the concrete baseline project/folder structure in `feature-model.md` before assigning `IsCommon` to any candidate feature.
  - Risk: without a literal baseline, commonality gets guessed from the catalog's current shape — "no plateau skips it" proves only that no one has needed to yet. (`EntityBehaviour` was wrongly marked common this way; the baseline showed the common layout has no Domain project at all.)
  - Fix: write the baseline first, in real project/folder names; test every candidate against it.
- **The root is product, not feature** - Name the diagram's root as the family's product, exclude it from the Features table, and group it inside the `Common` block.
  - Risk: an implicit or ungrouped root leaves every other node's parent ambiguous and forces a disruptive re-layout once the diagram grows.
  - Fix: name it in prose, place it inside `Common`.
- **One source for variable edges** - Connect every top-level variable feature to the `Common` block as a whole, never to the root individually.
  - Risk: one hub node fanning into many individual edges obscures the tree structure it is meant to show.
  - Fix: draw edges from the block's own subgraph id.
- **Avoid long vertical stacks** - Arrange more than ~4 members of one block as a row matrix (per [# The diagram](#the-diagram)) instead of a vertical list.
  - Risk: a long vertical stack of boxes reads as sprawl even after grouping.
  - Fix: nested `direction LR` rows in a `direction TB` block, cosmetic row borders hidden, visible border only for a real sub-feature group.
- **No unlabeled edges** - Label every edge with its exact relation type from the closed list in [# The diagram](#the-diagram).
  - Risk: a relation implied only by a node's own text is easy to miss and cannot be checked mechanically later.
  - Fix: use mermaid's edge-label syntax on every single edge, no exceptions.
- **Spell out parallel-Requires logic** - State the AND/OR relationship in prose whenever two or more `Requires` edges point at the same target.
  - Risk: two parallel dotted edges look identical whether the real rule is AND or OR; nothing in the diagram disambiguates.
  - Fix: add one sentence naming the exact logic.
- **One mechanism, one feature** - Verify two candidate features are not the same technical mechanism under different names before modeling them separately.
  - Risk: "Command" and "Query" modeled separately while sharing one dispatch mechanism invent variability that is not real and must be un-modeled later.
  - Fix: check the actual mechanism each candidate would use, not its business-facing name.
- **Infrastructure is the owner's call** - Ask the family's owner before excluding a fixed-seeming capability (exception handling, logging, build/test gates) from the model as "just infrastructure."
  - Risk: silent exclusion hides a capability the owner may want to grow later. (`AppLogging`, `ExceptionHandlingPipeline`, and `TestConformance` were excluded this way, then added back as explicit common features once asked.)
  - Fix: ask; do not default to exclusion.
- **Reasoning-only constraints stay provisional** - Flag a cross-tree constraint as unconfirmed whenever it is architectural reasoning with no existing solution/pattern to check it against, and get it confirmed before treating it as settled.
  - Risk: an unverified constraint presented as settled can silently block a legitimate combination or hide a missing one.
  - Fix: mark it distinctly in prose (e.g. "flagged, not yet confirmed") until the owner confirms it.
- **Bundled children nest, optional children don't** - Nest a feature's children inside the parent's diagram block when they are unconditionally present together with it; draw them outside, connected by a labeled edge, when they are independently selectable.
  - Risk: flattening a decomposing feature into one leaf row loses which sub-parts exist and whether they are bundled or independently optional.
  - Fix: `TestConformance`-shaped groups (bundled, nested inside their already-common parent's block) vs `Persistence`-shaped groups (independently optional, drawn outside with `Optional` edges).
- **Selectors and companions get prose, not rows** - Exclude a pure selector (maps combinations of other features, no independent Yes/No of its own) and a mandatory companion (always applied together with another feature, never independently) from the Features table; explain the exclusion in a short prose note instead of a row.
  - Risk: giving a selector or a mandatory companion its own row implies it is an independent choice, which it never is.
  - Fix: name it explicitly in prose under the table, next to the feature(s) it belongs to.
- **Follow the repo diagram convention** - Store the diagram as `diagrams/feature-diagram.mmd` and embed it via `@import`, per [[skills/common-workflow/mermaid-diagram.skill.md|mermaid-diagram]], once it exceeds ~5 elements.
  - Risk: a plain fenced code block at this size violates the repository's diagram convention and won't render in the required preview tooling.
  - Fix: separate file, `diagrams/` subfolder, `@import` directive.
- **Out of scope covers every limitation** - Write an `Out of scope` section covering: what is excluded as fixed infrastructure and why; how Plateau Components differ and are excluded; whether the model targets an existing catalog only or the full intended Program Family; which constraints are unverified; that `IsCommon` verdicts are judgment calls, not proofs.
  - Risk: without it, a reader cannot tell a deliberate limitation from an oversight.
  - Fix: fill the section with real content every time; never leave it as a placeholder.
- **One change at a time** - Confirm each materially new or changed part of the model with the family's owner before building further on it.
  - Risk: a wrong assumption compounds silently across several additions before anyone notices.
  - Fix: present one change at a time and get confirmation before the next.
- Follow [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
  - Risk: this skill's rules cover the Feature Model's content, not the mechanics every skill must follow — skipping the shared baseline produces a technically-correct model in a non-conforming skill file.
  - Fix: apply `skill-design.skill.md` in addition to, never instead of, the rules above.

## SHOULD
- **Drop duplicates kept for history** - Rename or merge features once a deeper technical read reveals they are the same mechanism, rather than keeping both for historical reasons.
- **No self-invented synonyms** - Reuse vocabulary the family's owner has already stated a preference for (e.g. `Optional (at least one)`) instead of a self-invented synonym for the same relation.

## MAY
- **Exactly-one-of-N may not apply yet** - Leave the reserved relation type (`Alternative (group name)`) unused in the diagram when no feature set in the current model actually needs exactly-one-of-N cardinality.

# Check list
- [ ] The baseline project/folder structure is written out in `feature-model.md`, in real names, before any `IsCommon` verdict.
- [ ] The diagram's root is named explicitly, grouped inside `Common`, and absent from the Features table.
- [ ] Every top-level variable feature connects to the `Common` block itself, not to the root individually.
- [ ] Any block with more than ~4 members is arranged as a row matrix, with cosmetic row borders hidden and real sub-feature group borders visible.
- [ ] Every edge in the diagram carries an explicit relation label (`Mandatory`/`Optional`/`Optional (at least one)`/`Alternative (group name)`/`Requires`).
- [ ] Any parallel `Requires` edges into one target have their AND/OR logic stated in prose.
- [ ] No two features model the same underlying technical mechanism under different names.
- [ ] Every fixed-seeming capability excluded as "infrastructure" was checked with the family's owner first.
- [ ] Every unconfirmed cross-tree constraint is flagged distinctly, not presented as settled.
- [ ] Every decomposing feature's children are nested (bundled parent) or external-with-labeled-edge (independently optional parent), matching which kind they are.
- [ ] Every selector and mandatory companion is excluded from the table with a prose explanation, not given its own row.
- [ ] The diagram lives at `diagrams/feature-diagram.mmd` and is embedded via `@import`, per `mermaid-diagram.skill.md`.
- [ ] `Out of scope` is filled with real content covering fixed infrastructure, Plateau Components, existing-catalog-vs-intended-family scope, unverified constraints, and the judgment-call nature of `IsCommon`.
- [ ] Each material change was confirmed with the family's owner before the next was built on top of it.
- [ ] Facet tags follow [[skills/common-workflow/skill-design.skill/facet-vocabulary.md|facet-vocabulary]]: `concern/architecture`, bare `stack`.

# Examples
- [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/examples/example-dotnet-feature-model.md|.NET worked example]] — built live while writing this skill; the real, complete result is [[skills/dotnet/architecture/v3.1/feature/feature-model.md|feature-model.md]].
