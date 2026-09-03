# design-system plateaus

One plateau — the catalogue has **no VPs** ([`../variability-map.md`](../variability-map.md)).

| Plateau | `standalone` | Parent | Composes |
|---|---|---|---|
| **plateau-design-system** | `true` | — (from scratch) | all four common solutions: `solution-design-system-structure`, `-tokens`, `-components`, `-ui-testing` |

**VP1** MultiTenantTheming is aspirational — `solution-design-system-multi-tenant-theming` is a
`> Draft contract` skeleton; if built, `HybridDesignTokens` becomes the single-tenant variant of a
`Theming` VP and this catalogue gets its first second plateau.

Run `bash skills/angular/architecture/v3.1/agent/check.sh` after any change.

## What the plateau folder holds

```
plateau-design-system/
  plateau-design-system.skill/
    plateau-design-system.skill.md   the plateau summary
    example/                          a plain Angular CLI multi-project workspace (NOT Nx)
  structure/                          repo + 2 project skills + 8 class skills (prefix plateau-design-system--)
  registry/                           one entry: design-system-repository
```

- **11 structure skills**: `repo-design-system` + `project-design-system` (the ng-packagr library) +
  `project-demo` (the unpublished preview app / visual-a11y target) + `class-theme`, `class-custom-tokens`,
  `class-component-name`, `class-read-visual-style-properties`, and the four spec patterns
  (`component` / `visual` / `style-snapshot` / `a11y`).
- **example gates**: `ng build design-system` (Angular Package Format) + `ng test design-system`
  (Vitest via Angular 22's native `@angular/build:unit-test`, 2 files / 7 tests) + `ng build demo`
  + `tsc -p tsconfig.e2e.json` — all green. `grep "@angular/material" dist/design-system/types/`
  returns nothing. Playwright specs written, not run.

## `registry/`

- **`design-system-repository`** — `solution-design-system-structure` `.create` + `-tokens` /
  `-components` / `-ui-testing` `.extend` (each adds one distinct piece — SCSS entry points, the
  `ds-*` authoring convention, the `projects/demo` visual/a11y targets). `FMN`/`TMN`,
  `source: ordering-only`, **N = 4 benign** — the analogue of the monolith's `monolith-repository`.

Full classification: [`../../delta-conflict-analysis.md`](../../delta-conflict-analysis.md).
