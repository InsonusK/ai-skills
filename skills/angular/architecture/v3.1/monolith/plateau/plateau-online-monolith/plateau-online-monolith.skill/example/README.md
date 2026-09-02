# plateau-online-monolith — reference example

The runnable Nx workspace for `plateau-online-monolith`. One feature (`orders`) end to end.

| Plateau piece | Where |
| --- | --- |
| Nx workspace, apps/libs split, type:*/scope:* tags, enforced boundaries | nx.json, eslint.config.mjs, project.json |
| apps/platform-shell — shell, app.routes.ts mounts orders lazily | apps/platform-shell/src/app/ |
| apps/component-preview — visual/a11y target (static preview routes) | apps/component-preview/src/app/previews/ |
| State tiering — component signal + feature Signal Store; empty-but-wired global NgRx store | libs/orders/feature/src/lib/orders.store.ts, libs/shared/state/src/lib/store.config.ts |
| libs/shared/http-core — base HTTP service (base URL, timeout, retry on GET only) | libs/shared/http-core/ |
| libs/orders/data-access — Facade -> Client -> http-core, typed errors, hand-written mapper | libs/orders/data-access/src/lib/ |
| libs/orders/feature — Signal Forms form, OrdersStore calls the Facade directly (optimistic creating -> created) | libs/orders/feature/src/lib/ |
| libs/shared/logging — LoggerService + ConsoleLogSink + LOG_SINKS seam | libs/shared/logging/src/lib/ |
| four-layer component tests | libs/orders/feature/src/lib/*.spec.ts + .../spec/ |

## Running

    npm install
    npm test          # Vitest, jsdom — all 9 projects green (the plateau ground-truth gate)
    npm run lint
    npx playwright install chromium
    npm run e2e        # apps/platform-shell-e2e, API mocked via page.route
    npx nx run component-preview:serve --port=4300 &
    npm run e2e:visual # visual regression + a11y against apps/component-preview

## Verified state

`npm test` and `npm run lint` are GREEN. The Playwright suites (e2e, e2e:visual) are written and
configured but were not executed where this example was built (the Playwright test-runner could not
fork workers there; chromium.launch() itself worked). Run them in a normal CI/dev environment.

## Fed back into the catalog

solution-api-http-layer's rule "retry applies to idempotent (GET) requests only" is real: an early
draft of base-http.service.ts that retried POST doubled a 409'd write. base-http.service.spec.ts pins
GET-only retry.
