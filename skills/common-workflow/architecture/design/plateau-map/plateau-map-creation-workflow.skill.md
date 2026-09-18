---
name: plateau-map-creation-workflow
description: Orientation for the plateau-map pipeline — the fixed order in which feature-map-create, variability-map-create, delta-conflict-detection, plateau assembly, and plateau-map-create turn a Program Family's common/variable split into named, buildable plateaus, so an agent knows where to start instead of inferring it from cross-links scattered across the skill files
whenToUse: when starting work on a new plateau/solution catalog from scratch and unsure which skill to run first, when asked what the plateau-map pipeline is or what this directory contains, or when reviewing whether a catalog change skipped a required earlier or later stage
updated: 20260918
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
---

# Goal
- One page naming, in the order they run, the skills that turn an unstructured sense of "what this Program Family shares vs. varies" into named, buildable plateaus — so starting a catalog from scratch (or resuming one mid-pipeline) means reading one page instead of inferring the shape from cross-links scattered across the skill files.
- No content of its own: what each stage produces, how, and under what rules is stated once, in that stage's own skill file; this file states only the order and the two triggers that re-enter it.

# Core Principle
- **Sequencing only, never restating** - Every fact about how to build a Feature Model, fill a Variability Map, classify an intersection, or assemble a plateau lives in that stage's own skill; this file links to it rather than repeating it — the same discipline [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]] itself applies to the Variability Map, one level up.
- **Four skills own this pipeline; two more are called into it** - `feature-map-create`, `variability-map-create`, `delta-conflict-detection`, and `plateau-map-create` are the skills this pipeline introduces — an ordered read of "what varies here" that exists nowhere else. Plateau assembly (`plateau-create-by-solutions`/`plateau-update-by-solutions`) and solution authoring are general-purpose skills this pipeline calls into, not stages unique to it.
- **Two step shapes** - Steps 1–3 and 5 run once per catalog-level change (a new VP, a new constraint). Step 4 (assembling or updating a plateau) recurs throughout the catalog's life, not only once per pipeline pass — `solution-update` explicitly re-invokes it via `plateau-update-by-solutions` whenever a solution it updates reaches into an existing plateau.

# Workflow

## The five steps
1. **[[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]** — write the Program Family's common/mandatory baseline vs. its variable feature set as a FODA-style diagram + table, grounded in a concrete baseline project structure. Output: `{catalog}/feature/feature-model.md`.
2. **[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]]** — group the Feature Model's non-common features into Variation Points and fill each row's VP, Variants, Constraint, Realization-depends-on, and Migration columns. Output: `{catalog}/variability-map.md`. The `Realized by` column is filled by step 3, run as a required sub-step of this one.
3. **[[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]]** — fill the Variability Map's `Realized by` column: walk each VP, author or reuse the solution(s) that realize it, then classify every group of solutions that touch the same code element (`Constraint x Category x Kind`) and build a resolver only for the codes that need one. Output: the finished `Realized by` column, plus per-element files in the plateau's own `registry/` folder. Runs as the last step of step 2 — the map is not finished without it.
4. **Assemble the plateaus** — via `plateau-create-by-solutions` (a new plateau, from a Variability Map row's Realized-by combination) or `plateau-update-by-solutions` (an existing plateau gains or loses a solution). Not a skill of this pipeline's own folder — it runs repeatedly against the catalog, not only once per pipeline pass. Output: `{catalog}/plateau/plateau-{name}/`.
5. **[[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]]** — maintain the plateau↔VP view: recompute `{catalog}/plateau/plateau-repository.md` (the Plateau × VP matrix and lineage) whenever a plateau changes (step 4) or a VP changes (step 2), cross-checking every plateau's VP set against the map's Constraints. Assumes a fully-filled `variability-map.md`. Output: `{catalog}/plateau/plateau-repository.md`.

## Where each stage's output lives, per catalog
```
{stack}/architecture/{catalog}/
  feature/
    feature-model.md          ← step 1
    diagrams/feature-diagram.mmd
  variability-map.md          ← step 2 (Realized by column filled by step 3)
  solutions/
    solution-{name}.skill/    ← step 3, one per Realized-by entry
  plateau/
    plateau-repository.md      ← step 5 — the plateau↔VP matrix, recomputed on plateau/VP changes
    plateau-{name}/
      plateau-{name}.skill/
        plateau-{name}.skill.md
        example/
      structure/
      registry/                ← step 3's output, placed at the shallowest plateau where it's real
      adr/
```

# Scope

## Not part of this pipeline
- [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] — Solution vs. Plateau vs. Plateau Component: which shape a new architectural unit should take.
- [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]] — building a Plateau Component (an optional, cross-cutting capability attached after composition, never part of `created_by`).
- [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md|adr-create]] — recording an architecture decision as an ADR; used throughout every step above, not specific to this pipeline.

# Rule

## MUST

### Follow the fixed order
Run `feature-map-create`, then `variability-map-create` (with `delta-conflict-detection` as its required sub-step), before assembling any plateau, before running `plateau-map-create`.
- Violation: naming plateaus or writing solutions before a Feature Model and a fully-filled Variability Map exist for the catalog.
- Risk: a plateau assembled ahead of the map encodes one team's guess at the catalog's variability instead of a reasoned common/variable split, and has to be reverse-engineered back into a map later instead of the other way round.
- Fix: treat steps 1–3 as a precondition for step 4, exactly as each of those steps' own skill files state in their own precondition rules.

### State no stage's content here
Never restate a rule, workflow step, template, or output shape that belongs to one of the five linked skills — link to it instead.
- Risk: two homes for one fact drift apart the moment either is edited — the exact failure each linked skill's own "never restate" rule (e.g. `plateau-map-create`'s "Derived from the map, never restating it") exists to avoid, one level up.
- Fix: keep this file to the order, the triggers, and the output-location map; content lives only in the stage's own skill.

### Follow the skill-design baseline
Follow [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
- Risk: an orientation skill is still a skill an agent must be able to find and follow; skipping the shared baseline makes it as hard to trust as any other non-conforming skill file.
- Fix: apply `skill-design.skill.md` in addition to, never instead of, the rules above.

# Check list
- [ ] The five steps are listed in their fixed order, each linking to its own skill file, with no stage's own content restated here.
- [ ] The "Not part of this pipeline" list names only skills that actually exist in the repository — verify each link resolves before adding it.
- [ ] Facet tags follow [[skills/design/skill-tags.skill/skill-tags.skill.md|skill-tags]]: `concern/architecture`, bare `stack`.
