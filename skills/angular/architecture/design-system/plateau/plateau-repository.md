# design-system plateaus

Two plateaus — the catalogue's one VP is `MultiTenantTheming` ([`../variability-map.md`](../variability-map.md)).

| Plateau | `standalone` | Parent | Composes / adds |
|---|---|---|---|
| **plateau-design-system** | `true` | — (from scratch) | all four common solutions: `solution-design-system-structure`, `-tokens`, `-components`, `-ui-testing` |
| **plateau-multi-tenant-design-system** | `true` | plateau-design-system | + `solution-design-system-multi-tenant-theming` (**VP1 = Yes**) — a `styles/tenants/` layer of swappable per-tenant palettes |

`plateau-multi-tenant-design-system` generalizes the single fixed brand palette: `styles/theme.scss`
is unchanged and is the no-`data-tenant` default; each tenant is one `:root[data-tenant='<id>']` file
that overrides **colour only** via a shared `ds-tenant-theme` mixin; the consuming app selects a tenant
with `document.documentElement.dataset.tenant` against the exported `DsTenant` union.

Run `bash skills/angular/architecture/v3.1/agent/check.sh` after any change.

## Plateau × VP matrix

The catalog has one VP — `MultiTenantTheming` ([`../variability-map.md`](../variability-map.md)).
Answers are cumulative down the lineage.

| Plateau | Parent | VP1 |
|---|---|:-:|
| plateau-design-system | — (from scratch) | ❌ |
| plateau-multi-tenant-design-system | plateau-design-system | ✅ |

Column legend — VP1 MultiTenantTheming (requires `HybridDesignTokens`; realized by
`solution-design-system-multi-tenant-theming`). Full description in
[`../variability-map.md`](../variability-map.md).

## Reference: V1 → v3.1

| V1 plateau | v3.1 plateau | VP1 |
|---|---|:-:|
| `plateau-design-system` | `plateau-design-system` | ❌ — composes all four common solutions |
| *(none)* | `plateau-multi-tenant-design-system` | ✅ — + `solution-design-system-multi-tenant-theming` |

`plateau-multi-tenant-design-system` is the catalog's first plateau with no V1 equivalent — a
multi-tenant-themed design system, built on `plateau-design-system` via `parent_plateaus`.

## What each plateau folder holds

```
plateau-{name}/
  plateau-{name}.skill/
    plateau-{name}.skill.md   the plateau summary
    example/                   a plain Angular CLI multi-project workspace (NOT Nx)
  structure/                   repo + 2 project skills + class skills (prefix plateau-{name}--)
  registry/                    one entry: design-system-repository
```

| Plateau | structure skills | example gates |
|---|---|---|
| plateau-design-system | 11 | `ng build design-system` (APF) + `ng test` (Vitest via `@angular/build:unit-test`, 2 files / 7 tests) + `ng build demo` + `tsc -p tsconfig.e2e.json`; no `@angular/material` in `dist/**/types/` |
| plateau-multi-tenant-design-system | 14 (+ `class-tenant-theme`, `class-tenant-palette`, `class-tenants`) | + `styles/tenants/**` shipped as assets, `DsTenant` in the typings; `ng test` 3 files / 9 tests; `ng build demo` root CSS carries `:root[data-tenant='acme']` / `[data-tenant='globex']` |

Playwright specs (incl. the per-tenant style-snapshot) are written and configured throughout but were
not executed in the build sandbox. The design-system CLI workspace has no lint target.

## `registry/`

- **`design-system-repository`** — `solution-design-system-structure` `.create` + `-tokens` /
  `-components` / `-ui-testing` `.extend` (each adds one distinct piece), plus (at
  `plateau-multi-tenant-design-system`) `-multi-tenant-theming` `.extend` — the `styles/tenants/`
  layer, entirely in new files under a new directory. `FMN`/`TMN`, `source: ordering-only`,
  **N = 4 benign** at `plateau-design-system`, **N = 5 benign** at `plateau-multi-tenant-design-system`
  — the analogue of the monolith's `monolith-repository`.

Full classification: [`../../delta-conflict-analysis.md`](../../delta-conflict-analysis.md).
