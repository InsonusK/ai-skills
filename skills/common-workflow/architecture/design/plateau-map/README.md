# plateau-map pipeline

Orientation only — **this file is not a skill.** It has no frontmatter/`whenToUse`, is never loaded
by the harness, is not shipped to `.claude/skills`, and is not consumed by another repository that
depends on skills from this one. It exists so a human or an agent asked "разберись, что в этой
директории находится" (figure out what's in this directory) has one place to read instead of
inferring the pipeline shape from cross-links scattered across three skill files.

The three skills in this folder turn an unstructured sense of "what this Program Family shares vs.
varies" into named, buildable plateaus — in this order:

## The pipeline

1. **[feature-map-create](feature-map-create.skill/feature-map-create.skill.md)** — write the Program
   Family's common/mandatory baseline vs. its variable feature set as a FODA-style diagram + table,
   grounded in a concrete baseline project structure. Output: `{catalog}/feature/feature-model.md`.
2. **[variability-map-create](variability-map-create.skill/variability-map-create.skill.md)** — group
   the Feature Model's non-common features into Variation Points (one row each: Variants, Constraint,
   Realized-by solution, Migration), and derive the catalog's named plateaus as specific points in
   that combination space. Output: `{catalog}/variability-map.md`.
3. **Author the solutions each VP's Realized-by column names** — via
   [../solution-create.skill/](../solution-create.skill/solution-create.skill.md) (new) or
   [../solution-update.skill/](../solution-update.skill/solution-update.skill.md) (existing). Not in
   this folder: a solution is created/updated on its own throughout a catalog's life, not only as a
   step of this pipeline. Output: `{catalog}/solutions/solution-{name}.skill/`.
4. **[delta-conflict-detection](delta-conflict-detection.skill/delta-conflict-detection.skill.md)** —
   classify every pair of solutions that touch the same code element (`Constraint x Category x Kind`),
   and build a resolver only for the three codes that need one. Output: per-element files in the
   plateau's own `registry/` folder.
5. **Assemble the plateau** — via
   [../plateau-create-by-solutions.skill/](../plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md)
   (new plateau, from a Variability Map row's Realized-by combination) or
   [../plateau-update-by-solutions.skill/](../plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md)
   (an existing plateau gains/loses a solution). Not in this folder, for the same reason as step 3 —
   these run repeatedly against the catalog, not only once per pipeline pass. Output:
   `{catalog}/plateau/plateau-{name}/`.

Steps 1–2 and 4 run once per catalog-level change (a new VP, a new constraint). Steps 3 and 5 run
once per solution/plateau and recur throughout the catalog's life — [solution-update](../solution-update.skill/solution-update.skill.md)
explicitly re-invokes step 5 (via `plateau-update-by-solutions`) whenever a solution it updates
reaches into an existing plateau.

## Why these three are grouped here, not the other design/ skills

Steps 1, 2, and 4 are the ones this pipeline actually introduces — an ordered, three-stage read
of "what varies here" that exists nowhere else in the repository. Solution and plateau
authoring/updating (steps 3 and 5) are general-purpose skills this pipeline *calls into*, not stages
unique to it — see [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]
for how a Solution, a Plateau, and a Plateau Component relate, independent of this pipeline.

## Related, not part of this pipeline

- [../solution-plateau-hierarchy.skill.md](../solution-plateau-hierarchy.skill.md) — Solution vs.
  Plateau vs. Plateau Component: which shape a new architectural unit should take.
- [../plateau-component-create.skill/](../plateau-component-create.skill/plateau-component-create.skill.md) —
  building a Plateau Component (an optional, cross-cutting capability attached after composition,
  never part of `created_by`).
- [../solution-dependency-canvas-update.skill/](../solution-dependency-canvas-update.skill/solution-dependency-canvas-update.skill.md) —
  keeping `*.canvas` diagrams of solution/plateau relationships in sync after a `depends_on`/
  `parent_plateaus`/`built_on_plateau` change.
- [../adr-create.skill/](../adr-create.skill/adr-create.skill.md) — recording an architecture
  decision as an ADR; used throughout every step above, not specific to this pipeline.

## Where each stage's output lives, per catalog

```
{stack}/architecture/{catalog}/
  feature/
    feature-model.md          ← step 1
    diagrams/feature-diagram.mmd
  variability-map.md          ← step 2
  solutions/
    solution-{name}.skill/    ← step 3, one per Realized-by entry
  plateau/
    README.md                 ← human-facing index for this catalog's plateaus (not a skill either)
    plateau-{name}/
      plateau-{name}.skill/
        plateau-{name}.skill.md
        example/
      structure/
      registry/                ← step 4's output, placed at the shallowest plateau where it's real
      adr/
```

Worked example of the whole pipeline against a real catalog:
[`skills/angular/architecture/v3.1/`](../../../../angular/architecture/v3.1/README.md) (four
catalogs — `monolith/`, `design-system/`, `platform-host/`, `embeddable-app/` — each with its own
`feature/`, `variability-map.md`, and `plateau/`) and
[`skills/dotnet/architecture/v3.1/`](../../../../dotnet/architecture/v3.1/plateau/README.md).
