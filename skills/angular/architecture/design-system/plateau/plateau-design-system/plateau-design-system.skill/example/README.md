# plateau-design-system — reference example

A plain **Angular CLI multi-project workspace** (not Nx) that composes the four common
design-system solutions: `solution-design-system-structure`, `-tokens`, `-components`,
`-ui-testing`. It is the standalone, independently versioned npm package that `monolith`,
`platform-host` and `embeddable-app` consume.

## Layout

| Piece | Where |
| --- | --- |
| Publishable library (ng-packagr → Angular Package Format) | `projects/design-system/` |
| Single `mat.theme()`, one fixed brand palette, `light-dark()` | `projects/design-system/styles/theme.scss` |
| `--ds-*` tokens for gaps Material doesn't model (status/priority colour, spacing, radius) | `projects/design-system/styles/custom-tokens.scss` |
| `DsButtonComponent` — signal API, **delegates to** `matButton` internally | `projects/design-system/src/lib/button/button.component.ts` |
| `DsStatusChipComponent` — **fully custom**, consumes `--ds-color-status-*` | `projects/design-system/src/lib/status-chip/status-chip.component.ts` |
| Behavioural specs (Vitest + Testing Library, nothing to fake) | `src/lib/{component}/spec/{component}.component.spec.ts` |
| Visual / style-snapshot / a11y specs (Playwright, target = `projects/demo`) | `src/lib/{component}/spec/{component}.{visual,style-snapshot,a11y}.spec.ts` |
| Preview components (authored next to the component, imported by the demo) | `src/lib/{component}/spec/preview/{component}.preview.ts` |
| Shared `readVisualStyleProperties` helper (one curated list, all components) | `projects/design-system/testing/read-visual-style-properties.ts` |
| Unpublished preview app, doubles as the visual/a11y target | `projects/demo/` |
| Changesets versioning; `demo` is `ignore`d | `.changeset/` |

## Running

    npm install
    npm run build:lib   # ng build design-system — ng-packagr, Angular Package Format output in dist/design-system
    npm test            # ng test design-system — Vitest + jsdom, 2 files / 7 tests green
    npm run build:demo  # ng build demo — consumes design-system (theme + tokens) at the root
    npx playwright install chromium
    npm run e2e         # Playwright: visual + style-snapshot + a11y against a served demo
    npm run changeset   # add a changeset for a library-touching change

## Verified state

`npm run build:lib` (ng-packagr APF), `npm test` (7 behavioural tests), `npm run build:demo`
(production), and `tsc -p tsconfig.e2e.json` (Playwright specs typecheck) are all GREEN.
`grep -r "@angular/material" dist/design-system/types/` returns nothing — no Material type
reaches the published surface. The Playwright runner cannot fork workers in the sandbox this
example was built in, so `spec/snapshot/` baselines are generated where CI runs; the specs
and `playwright.config.ts` (with `snapshotPathTemplate` → `spec/snapshot/`) are complete.

## Fed back into the catalog

- **A Material type leaks into the ng-packagr `.d.ts` even from a `protected` field.**
  `DsButtonComponent` first typed its internal `matAppearance` computed as
  `MatButtonAppearance` (imported from `@angular/material/button`). ng-packagr emits every
  member — including `protected` ones — into `types/design-system.d.ts`, so
  `import { MatButtonAppearance } from '@angular/material/button'` appeared in the public
  typings, violating the "no Material type in the public API surface" MUST. Fix: internal
  Material-mapping helpers use a **local literal type** (`'filled' | 'outlined' | 'text'`),
  never a Material-exported one. The encapsulation rule needs a check against the built
  `types/*.d.ts`, not just the `.ts` source.
- **Angular 22's `@angular/build:unit-test` builder replaces the Analog Vitest setup.**
  The design-system CLI workspace uses the native builder (`runner: vitest`, jsdom) with a
  `setupFiles` entry for `@testing-library/jest-dom/vitest` and an `include` narrowed to
  `**/*.component.spec.ts` so the Playwright specs under `spec/` are not picked up. No
  `vite.config` / Analog plugin needed (unlike the monolith plateaus, built before the
  builder shipped).
- **Preview components import the *published* package (`from 'design-system'`), not the
  sibling source.** This keeps `projects/demo` a real consumer of the built artifact and
  makes each preview a lightweight check that the public API alone is enough. They stay in
  `src/lib/{component}/spec/preview/` (per `solution-design-system-ui-testing`) and are
  excluded from the library build (`tsconfig.lib.json` excludes `**/spec/**`).
- **`theme.scss` / `custom-tokens.scss` ship as package assets** at `design-system/styles/`
  (`ng-package.json` `assets`), and a consumer resolves them with Sass
  `includePaths: ["dist"]` + `@use 'design-system/styles/theme'`.
