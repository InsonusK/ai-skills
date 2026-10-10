# plateau-multi-tenant-design-system — reference example

The `plateau-design-system` workspace, evolved one plateau further: it composes the four common
design-system solutions **plus** `solution-design-system-multi-tenant-theming` (design-system
**VP1 — MultiTenantTheming**). Everything from the parent plateau still applies (see its README).
This file records only the VP1 delta.

**No new project.** VP1 adds a `styles/tenants/` layer and one small TS module to
`projects/design-system`, and a tenant switcher to `projects/demo`.

## VP1 — MultiTenantTheming (`solution-design-system-multi-tenant-theming`)

| VP1 piece | Where |
| --- | --- |
| `ds-tenant-theme($primary, $ds-overrides)` — the one place `mat.theme()` is called for a tenant; passes **only** the `color` key (so `styles/theme.scss` stays the sole source of typography/density) with `theme-type: color-scheme` (so `light-dark()` is preserved) | `projects/design-system/styles/tenants/_tenant-theme.scss` |
| One `:root[data-tenant='<id>']` block per tenant — `_acme.scss` (`mat.$violet-palette`), `_globex.scss` (`mat.$cyan-palette` + a teal `--ds-color-status-in-progress`) | `projects/design-system/styles/tenants/_acme.scss`, `_globex.scss` |
| `tenants.scss` — aggregator (`@use 'tenants/acme'; @use 'tenants/globex';`), shipped as the package asset a consumer imports **after** `theme` | `projects/design-system/styles/tenants.scss` |
| `ng-package.json` `assets` gains `./styles/tenants.scss` + `./styles/tenants` | `projects/design-system/ng-package.json` |
| `DS_TENANTS` (`['acme', 'globex'] as const`) + `DsTenant` union — the typed contract, exported from `public-api` | `projects/design-system/src/lib/tenants.ts` → `src/public-api.ts` |
| Demo tenant switcher — a `<select data-testid="tenant-select">` that sets / clears `document.documentElement.dataset.tenant`; models the consuming-app responsibility (the library ships no such control) | `projects/demo/src/app/app.ts` |
| `@use 'design-system/styles/tenants'` after `theme` in the demo root styles | `projects/demo/src/styles.scss` |
| Per-tenant style-snapshot spec — the same component, same DOM, only token *values* change; plus an assertion the font token is identical across tenants | `projects/design-system/src/lib/status-chip/spec/status-chip.tenant.style-snapshot.spec.ts` |

## Running

    npm install
    npm run build:lib   # ng build design-system — APF; dist/design-system/styles/tenants/** shipped; DsTenant in types/*.d.ts
    npm test            # ng test design-system — Vitest + jsdom, 3 files / 9 tests green (adds the DS_TENANTS shape spec)
    npm run build:demo  # ng build demo — the compiled root CSS carries :root[data-tenant='acme'] / [data-tenant='globex']
    npx playwright install chromium
    npm run e2e         # Playwright: visual + style-snapshot (incl. per-tenant) + a11y against a served demo

## Verified state

`npm run build:lib` (ng-packagr APF — `styles/tenants.scss` + `styles/tenants/_*.scss` copied as
assets; `DS_TENANTS` / `DsTenant` in `dist/design-system/types/design-system.d.ts`;
`grep -r "@angular/material" dist/design-system/types/` still returns nothing), `npm test`
(3 files / 9 tests), `npm run build:demo` (295 kB; compiled root CSS contains
`:root[data-tenant=acme]` and `:root[data-tenant=globex]`), and `tsc -p tsconfig.e2e.json`
(the per-tenant Playwright spec typechecks) are all GREEN. The design-system CLI workspace has
no lint target (consistent with `plateau-design-system`). The Playwright runner cannot fork
workers in the sandbox this example was built in, so `spec/snapshot/` baselines are generated
where CI runs.

## Fed back into the catalog

- **The tenants asset must resolve as `styles/tenants.scss`, not `styles/tenants/tenants.scss`.**
  `@use 'design-system/styles/theme'` resolves to `dist/design-system/styles/theme.scss` — the
  pattern is `styles/<name>.scss`. So the aggregator sits at `styles/tenants.scss` and the
  partials in `styles/tenants/`; `ng-package.json` ships both the file and the directory. An
  aggregator nested one level deeper (`styles/tenants/tenants.scss`) would not resolve from the
  documented `@use 'design-system/styles/tenants'`.
- **A tenant re-emits the M3 colour token set only.** `ds-tenant-theme` passes `color` alone to
  `mat.theme()` — `styles/theme.scss` keeps emitting typography, density, and Material base
  styles once, at bare `:root`. The per-tenant `style-snapshot` spec asserts the resolved `font`
  is byte-identical across `acme` and `globex`, so "a tenant varies colour only" is a test, not a
  review rule.
- **`noPropertyAccessFromIndexSignature` forces `dataset['tenant']`.** The demo switcher reads /
  writes `document.documentElement.dataset['tenant']` (bracket form) under the CLI workspace's
  strict tsconfig — worth noting for any consumer copying the pattern.
