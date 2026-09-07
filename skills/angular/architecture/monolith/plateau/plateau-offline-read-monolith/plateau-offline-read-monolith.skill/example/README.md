# plateau-offline-read-monolith — reference example

The `plateau-async-monolith` workspace, evolved one plateau down the chain. Everything from the
parent chain (online-monolith + async-monolith) still applies — see those READMEs. This file records
only the VP4 delta — `solution-offline-first`.

| VP4 piece | Where |
| --- | --- |
| `connectivity` slice — `selectIsOnline = browserOnline && lastHealthCheckOk` | libs/shared/state/src/lib/connectivity/ (actions, reducer=`createFeature`, effects, selectors, spec) |
| Registered in the global store | libs/shared/state/src/lib/store.config.ts (`provideStore({})` + `provideState(connectivityFeature)` + `provideEffects(ConnectivityEffects)`) |
| Shared `OfflineTransportError` (defined once) | libs/shared/http-core/src/lib/offline-transport-error.ts |
| Client maps `status === 0` → `OfflineTransportError` before any status-code branch | libs/orders/data-access/src/lib/orders.client.ts (+ spec cases) |
| `OfflineBannerComponent` — presentational, `isOnline` input | libs/shared/ui/src/lib/offline-banner/ |
| Mounted once, shell wires the slice | apps/platform-shell/src/app/app.ts + app.html (`<ui-offline-banner [isOnline]="isOnline()">`) |
| Workbox SW — 4 content-type rules (network-only first) | apps/platform-shell/src/sw-src.ts |
| Routing predicates (unit-tested, DOM-safe) | apps/platform-shell/src/sw-routes.ts (+ sw-routes.spec.ts) |
| SW build step (esbuild + `workbox-build` injectManifest) | apps/platform-shell/src/sw-build.mjs, `build-sw` target in project.json (`dependsOn: [build]`) |
| SW tsconfig (WebWorker lib), excluded from the app build | apps/platform-shell/tsconfig.sw.json |
| `/sw.js` registered after bootstrap, prod only | apps/platform-shell/src/main.ts (`!isDevMode()` guard) |
| Offline acceptance specs (written, not run here) | apps/platform-shell-e2e/src/offline.e2e.spec.ts |

## Running

    npm install
    npm test          # Vitest, jsdom — 15 files / 44 tests green
    npm run lint      # nx run-many -t lint — 10 projects green
    npm run build:sw  # nx build-sw platform-shell — builds the app then generates dist/apps/platform-shell/browser/sw.js
    npx playwright install chromium
    npm run e2e        # apps/platform-shell-e2e — includes offline.e2e.spec.ts

## Verified state

`npm test`, `npm run lint`, and `npx nx build-sw platform-shell` are GREEN. The SW build reports
`sw.js: precached 7 files` and bundles the four Workbox routing strategies. Playwright suites
(`orders.e2e.spec.ts`, `offline.e2e.spec.ts`, visual/style/a11y) are written and configured but were
not executed where this example was built — the Playwright runner could not fork workers there.

## Fed back into the catalog

- `solution-offline-first`'s Implementation sketch injected `Store` **inside** `OfflineBannerComponent`.
  That would make `libs/shared/ui` (`type:ui`) depend on `libs/shared/state` (`type:store`), which the
  boundary allow-list forbids. This example makes the banner presentational (`isOnline` input) and moves
  the store wiring to the shell — cleaner and boundary-legal. The class skill records the deviation.
- The Workbox SW needs its own `tsconfig.sw.json` (`lib: ["webworker"]`) and must be excluded from
  `tsconfig.app.json` — a DOM-lib compile of `sw-src.ts` fails on `ServiceWorkerGlobalScope`. The
  routing *decisions* were extracted into a pure `sw-routes.ts` so they are unit-testable without a SW
  context.
- `sw-build.mjs` (not `sw-build.ts`) — a plain Node ESM script, since this workspace has no `.ts`
  script runner wired; it esbuild-bundles `sw-src.ts` first, then runs `workbox-build`.
