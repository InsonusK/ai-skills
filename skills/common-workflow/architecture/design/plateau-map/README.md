# plateau-map pipeline

Orientation only — **this file is not a skill.** It has no frontmatter/`whenToUse`, is never loaded
by the harness, is not shipped to `.claude/skills`, and is not consumed by another repository that
depends on skills from this one. It exists so a human or an agent asked "разберись, что в этой
директории находится" (figure out what's in this directory) has one place to read instead of
inferring the pipeline shape from cross-links scattered across the skill files.

The four skills in this folder turn an unstructured sense of "what this Program Family shares vs.
varies" into named, buildable plateaus — in this order:

## The pipeline

1. **[feature-map-create](feature-map-create.skill/feature-map-create.skill.md)** — write the Program
   Family's common/mandatory baseline vs. its variable feature set as a FODA-style diagram + table,
   grounded in a concrete baseline project structure. Output: `{catalog}/feature/feature-model.md`.
2. **[variability-map-create](variability-map-create.skill/variability-map-create.skill.md)** — group
   the Feature Model's non-common features into Variation Points and fill each row's VP, Variants,
   Constraint, Realization-depends-on, and Migration columns. The `Realized by` column is filled by
   step 3, run as a required sub-step of this one. The map binds VPs to solutions; it does not
   describe plateaus. Output: `{catalog}/variability-map.md`.
3. **[delta-conflict-detection](delta-conflict-detection.skill/delta-conflict-detection.skill.md)** —
   fill the Variability Map's `Realized by` column: walk each VP, author or reuse the solution(s) that
   realize it (via [../solution-create.skill/](../solution-create.skill/solution-create.skill.md) —
   draft contract when none exists yet), then classify every group of solutions that touch the same
   code element (`Constraint x Category x Kind`) and build a resolver only for the three codes that
   need one. Output: a fully-filled `Realized by` column, plus per-element files in the plateau's own
   `registry/` folder. Runs as the last step of step 2 — the map is not finished without it.
4. **Assemble the plateaus** — via
   [../plateau-create-by-solutions.skill/](../plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md)
   (new plateau, from a Variability Map row's Realized-by combination) or
   [../plateau-update-by-solutions.skill/](../plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md)
   (an existing plateau gains/loses a solution). Not skills of this folder — they run repeatedly
   against the catalog, not only once per pipeline pass. Output: `{catalog}/plateau/plateau-{name}/`.
5. **[plateau-map-create](plateau-map-create.skill/plateau-map-create.skill.md)** — maintain the
   plateau↔VP view: recompute `{catalog}/plateau/plateau-repository.md` (the Plateau × VP matrix and
   lineage) whenever a plateau changes (step 4) or a VP changes (step 2), cross-checking every
   plateau's VP set against the map's Constraints. Assumes a fully-filled `variability-map.md`.
   Output: `{catalog}/plateau/plateau-repository.md`.

Steps 1–3 run once per catalog-level change (a new VP, a new constraint). Step 4 runs once per
plateau and recurs throughout the catalog's life — [solution-update](../solution-update.skill/solution-update.skill.md)
explicitly re-invokes it (via `plateau-update-by-solutions`) whenever a solution it updates reaches
into an existing plateau. Step 5 runs after either kind of change.

## Why these four are grouped here, not the other design/ skills

feature-map-create, variability-map-create, delta-conflict-detection, and plateau-map-create are the
skills this pipeline actually introduces — an ordered read of "what varies here" that exists nowhere
else in the repository. Solution authoring/updating and plateau assembly are general-purpose skills
this pipeline *calls into*, not stages unique to it — see
[[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]
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

Worked example of the whole pipeline against a real catalog:
[`skills/angular/architecture/v3.1/`](../../../../angular/architecture/v3.1/README.md) (four
catalogs — `monolith/`, `design-system/`, `platform-host/`, `embeddable-app/` — each with its own
`feature/`, `variability-map.md`, and `plateau/`) and
[`skills/dotnet/architecture/v3.1/`](../../../../dotnet/architecture/v3.1/plateau/plateau-repository.md).
