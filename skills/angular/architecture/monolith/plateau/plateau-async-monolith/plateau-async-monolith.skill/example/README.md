# plateau-async-monolith — reference example

The `plateau-online-monolith` workspace, evolved one plateau down the chain. Everything from that
example still applies (see its README); this file records only the VP1 delta —
`solution-performance-tuned-routing`.

| VP1 piece | Where |
| --- | --- |
| `SelectivePreloadingStrategy` — preloads a chunk only on `data.preload === true` | apps/platform-shell/src/app/preloading/selective-preloading.strategy.ts (+ .spec.ts) |
| Router wired with `withPreloading(SelectivePreloadingStrategy)` | apps/platform-shell/src/app/app.config.ts |
| `orders` marked `data: { preload: true }` at the mounting point (not inside ORDERS_ROUTES) | apps/platform-shell/src/app/app.routes.ts |
| `loadComponent` sub-split of a heavy, rarely-visited screen into its own chunk | libs/orders/feature/src/lib/orders.routes.ts → order-report/order-report.component.ts |
| Guard spec: no route self-sets `preload`; report sub-route uses `loadComponent`, main path does not | libs/orders/feature/src/lib/orders.routes.spec.ts |
| `error`-level bundle budgets (`initial` + `anyScript`) on the production build | apps/platform-shell/project.json |

## Running

    npm install
    npm test          # Vitest, jsdom — all projects green (the plateau ground-truth gate)
    npm run lint      # nx run-many -t lint — 10 projects green
    npx nx build platform-shell --configuration=production   # budgets enforced; order-report-component is its own lazy chunk
    npx playwright install chromium
    npm run e2e        # apps/platform-shell-e2e, API mocked via page.route
    npm run e2e:visual # visual regression + a11y against apps/component-preview

## Verified state

`npm test` (Vitest), `npm run lint`, and `nx build platform-shell --configuration=production` are GREEN.
The production build output confirms the `loadComponent` split — `order-report-component` is emitted as
its own lazy chunk, absent from `main` and from the `orders` feature chunk. The Playwright suites (e2e,
e2e:visual) are written and configured but were not executed where this example was built (the
Playwright test-runner could not fork workers there; `chromium.launch()` itself worked).

## Fed back into the catalog

- Angular `anyScript` budgets apply to the initial `main` bundle too, not only lazy chunks — the first
  draft used a 300 kB `anyScript` error that failed on `main` (301 kB, NgRx + Angular). The example
  tunes `initial` and `anyScript` so both currently pass while still failing on a genuine regression.
- The `plateau-online-monolith` example's `nx lint` was not actually green (scaffold
  `enforce-module-boundaries` constraints); that fix landed on `plateau-online-monolith` and is
  inherited here. See that example's README.
