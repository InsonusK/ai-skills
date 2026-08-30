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
- Turn a fuzzy, ad-hoc sense of "what varies" into an explicit, reviewable artifact: a concrete baseline project structure, a diagram where every relation is typed per FODA, and a table with one row per feature stating whether it is common.
- Make the common/variable split defensible against a concrete, written-out baseline — never against "no existing plateau currently skips it," which proves nothing about whether skipping it is legitimate.
- Hand off a clean input to the next step in the pipeline: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] consumes this model's non-common features to build Variation Points and, from there, plateaus.

# Core Principle
- A feature earns a row only once tested against a concrete baseline structure written out in `feature-model.md` itself (real folder/project names) — not against vibes, and not against what an existing catalog's plateaus currently happen to share.
- The diagram and the table are two views of one model, kept in sync: every node in the diagram (except the root) has exactly one row in the table, and every table row appears somewhere in the diagram.
- Build this in small, reviewed increments with whoever owns the Program Family — a single unreviewed pass gets judgment calls wrong that only surface once someone looks closely (this skill exists because that happened, repeatedly, while working it out live).
- This skill's scope ends at the common/variable split and the diagram/table that express it. Grouping the variable features into Variation Points, mapping `Realized by`, and resolving conflicts are out of scope — see [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] and [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]].

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
2. **Write out the concrete baseline structure first** — the literal project/folder layout of a member of this family that has nothing but common features (real names: `App.Host`, `{Module}.Application`, `Shared`, `BuildingBlocks`, ...). This is not decoration; it is the test every later step measures a candidate feature against. If an existing catalog already exists, do not derive the baseline from "whatever every current plateau happens to include" — derive it from first principles (what must exist for the simplest legitimate member of the family to function at all), then check the existing catalog against it, not the other way round.
3. Enumerate candidate features from every source available: an existing catalog's solutions, and fresh requirements not yet built anywhere. Note explicitly which candidates are aspirational (no `Realized by` target exists yet) — this model describes the intended Program Family, not only what already happens to be implemented.
4. For each candidate, test it against the baseline from step 2: does supporting it require anything beyond the baseline as written? If yes, it is variable. If the baseline cannot function at all without it, it is common. Two traps found the hard way while building this skill:
   - Do not default a fixed-seeming capability (exception handling, logging, build/test conformance gates) to "infrastructure, not a feature" without asking the owner first — check whether they want it tracked as a first-class common feature, especially one expected to grow later (a minimal console logger today, file-backed later).
   - Before modeling two candidates as separate features, verify they are not the same underlying mechanism under two different names (a "Command" and a "Query" handled by the identical dispatch mechanism are one feature, not two, until something *technical* — not just semantic — actually differs).
5. Decide each feature's shape:
   - **Leaf** — no further decomposition.
   - **Group with further features** — decide whether its children are unconditionally present together with it (nest them in the same diagram block as their parent, e.g. a `TestConformance` group inside `Common`) or independently, separately selectable underneath it (draw them outside the block, connected by a labeled edge, e.g. `Persistence`'s three entity-level children).
6. Identify cross-tree constraints (a requirement between two features not connected by a direct parent-child edge). Verify each one against an existing solution/pattern when possible. Flag any constraint that is architectural reasoning alone, with nothing yet to check it against, as unconfirmed — do not present it with the same confidence as a verified one.
7. Build [`diagrams/feature-diagram.mmd`](#the-diagram) per the rules below.
8. Build `feature-model.md`: the baseline structure (step 2), the root/product explanation, the `@import`ed diagram, any boolean logic across parallel constraint edges the diagram itself can't express, the Features table, a note on anything deliberately excluded from the table, any flagged/unconfirmed constraints, and [Out of scope](#out-of-scope).
9. Confirm each materially new or changed part (a new feature, a rename, a constraint, a common/variable verdict) with the family's owner before building further on top of it.

# The diagram
Follow [[skills/common-workflow/mermaid-diagram.skill.md|mermaid-diagram]]: a diagram past ~5 elements is a separate `.mmd` file under `diagrams/`, embedded via `@import "./diagrams/feature-diagram.mmd" {as="mermaid"}` — never a plain fenced ```mermaid block at this size.

- **Root**: name the family's product explicitly (e.g. `Module`) as the diagram's root node. It is never a variability question and never a row in the Features table.
- **Common block**: wrap the root together with every `Mandatory` feature into one visually distinct `subgraph Common[...]`. Connect every top-level variable feature with an edge *from the `Common` block itself* (`Common -->|Optional| X`), never from the root individually — fanning a dozen-plus edges out of one node reads as clutter the moment the model grows past a handful of features.
- **Matrix layout inside a crowded block**: once a block (`Common`, or any other) holds more than ~4 members, arrange them as rows instead of a vertical list — nested subgraphs with `direction LR` for each row, `direction TB` on the outer block, and the row subgraphs' borders hidden (`style rowId fill:none,stroke:none`) so only the block's own border shows. Reserve a *visible* border for a real sub-feature grouping (e.g. `TestConformance`) — never hide the border of something that is an actual feature, only of a row that exists purely to lay nodes out.
- **Edge labels — every edge, no exceptions**: `Mandatory`, `Optional`, `Optional (at least one)` (a group where the parent's selection requires one or more of its children — this repository's own name for what FODA literature calls an "Or" group; use this exact phrase, not "Or"), `Alternative (group name)` (reserved: choose exactly one — not yet used in any model built with this skill, keep it available), or `Requires` (dotted, cross-tree, not a parent-child edge). Never leave a relation to be inferred only from a node's own label text.
- **Parallel `Requires` edges into one target**: state in `feature-model.md` prose, explicitly, whether they are jointly AND or OR — mermaid edge labels cannot express this on their own.

# Rule

## MUST
- Write out the concrete baseline project/folder structure in `feature-model.md` before assigning `IsCommon` to any candidate feature.
  - Risk: without a literal baseline, commonality gets guessed from an existing catalog's current shape — "no plateau currently skips it" — which proves only that no one has needed to yet, not that the baseline requires it. (This is exactly how `EntityBehaviour` was wrongly marked common in this skill's own first pass, before the baseline showed the common layout has no Domain project at all.)
  - Fix: write the baseline first, in real project/folder names; test every candidate against it.
- Name the diagram's root explicitly as the family's product, exclude it from the Features table, and group it inside the same `Common` block as the mandatory features.
  - Risk: an implicit or ungrouped root leaves every other node's parent ambiguous and forces a disruptive re-layout once the diagram grows (as happened here).
  - Fix: name it in prose, place it inside `Common`.
- Connect every top-level variable feature to the `Common` block as a whole, never to the root individually.
  - Risk: one hub node fanning into many individual edges obscures the tree structure it's meant to show.
  - Fix: draw edges from the block's own subgraph id.
- Arrange more than ~4 members of one block as a row matrix (nested `direction LR` rows in a `direction TB` block, row borders hidden) instead of a vertical list.
  - Risk: a long vertical stack of boxes reads as sprawl even after grouping.
  - Fix: nest rows, hide cosmetic row borders, keep a visible border only for a real sub-feature group.
- Label every edge with its exact relation type — `Mandatory`, `Optional`, `Optional (at least one)`, `Alternative (group name)`, or `Requires` — directly on the edge.
  - Risk: a relation implied only by a node's own text is easy to miss and cannot be checked mechanically later.
  - Fix: use mermaid's edge-label syntax on every single edge, no exceptions.
- State explicitly, in prose, the boolean relationship (AND/OR) whenever two or more `Requires` edges point at the same target.
  - Risk: two parallel dotted edges look identical whether the real rule is AND or OR; nothing in the diagram itself disambiguates.
  - Fix: add one sentence naming the exact logic.
- Verify two candidate features are not the same technical mechanism under different names before modeling them separately.
  - Risk: modeling "Command" and "Query" as separate features when both are the same request-dispatch mechanism invents variability that isn't real, and has to be un-modeled later once discovered.
  - Fix: check the actual mechanism each candidate would use, not only its business-facing name.
- Ask the family's owner before excluding a fixed-seeming capability (exception handling, logging, build/test gates) from the model as "just infrastructure."
  - Risk: silently excluding a real, trackable capability hides something the owner may want to grow later (this model's own `AppLogging`, `ExceptionHandlingPipeline`, and `TestConformance` were all excluded this way at first, then added back in as explicit common features once asked).
  - Fix: ask; do not default to exclusion.
- Flag a cross-tree constraint as unconfirmed whenever it is architectural reasoning with no existing solution/pattern to check it against, and get it confirmed before treating it as settled.
  - Risk: an unverified constraint presented with the same confidence as a verified one can silently block a legitimate combination or hide a missing one.
  - Fix: mark it distinctly in prose (e.g. "flagged, not yet confirmed") until the owner confirms it.
- Nest a feature's children inside the same diagram block as their parent when they are unconditionally present together with it; draw them outside, connected by a labeled edge, when they are independently, separately selectable.
  - Risk: flattening a decomposing feature into one leaf row loses real structure — which sub-parts exist, and whether they are bundled or independently optional.
  - Fix: model `TestConformance`-shaped groups (bundled, nested inside their already-common parent's block) differently from `Persistence`-shaped groups (independently optional, drawn outside with `Optional` edges).
- Exclude a pure selector (something that maps combinations of other features but has no independent Yes/No of its own) and a mandatory companion (something always applied together with another feature, never independently) from the Features table, and explain the exclusion in a short prose note instead of a row.
  - Risk: giving a selector or a mandatory companion its own row implies it is an independent choice, which it never is.
  - Fix: name it explicitly in prose under the table, next to the feature(s) it belongs to.
- Store the diagram as `diagrams/feature-diagram.mmd` and embed it via `@import`, per [[skills/common-workflow/mermaid-diagram.skill.md|mermaid-diagram]], once it exceeds ~5 elements.
  - Risk: a plain fenced code block at this size violates the repository's own diagram convention and won't render in the required preview tooling.
  - Fix: follow `mermaid-diagram.skill.md` exactly — separate file, `diagrams/` subfolder, `@import` directive.
- Write an `Out of scope` section covering: what is excluded as fixed infrastructure and why, how Plateau Components differ and are excluded, whether the model targets an existing catalog only or the full intended Program Family, which constraints are unverified, and that `IsCommon` verdicts are judgment calls, not proofs.
  - Risk: without this, a reader cannot tell a deliberate limitation from an oversight.
  - Fix: fill the section with real content every time; never leave it as a placeholder.
- Confirm each materially new or changed part of the model with the family's owner before building further on top of it.
  - Risk: a wrong assumption compounds silently across several additions before anyone notices.
  - Fix: present one change at a time and get confirmation before the next.

## SHOULD
- Rename or merge features once a deeper technical read reveals they are the same mechanism, rather than keeping both for historical reasons.
- Reuse vocabulary the family's owner has already stated a preference for (e.g. `Optional (at least one)`) instead of a self-invented synonym for the same relation.

## MAY
- Leave a reserved relation type (`Alternative (group name)`) unused in the diagram when no feature set in the current model actually needs exactly-one-of-N cardinality.

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

# Examples
- [[skills/dotnet/architecture/v3.1/design/feature-map-create.skill/examples/example-dotnet-feature-model.md|.NET worked example]] — built live while writing this skill; the real, complete result is [[skills/dotnet/architecture/v3.1/feature/feature-model.md|feature-model.md]].
