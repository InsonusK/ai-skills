# plateau-offline-full-monolith — reference example

The `plateau-offline-read-monolith` workspace, evolved one plateau down the chain — **the owner's
current app shape**. Everything from the parent chain still applies (see those READMEs). This file
records only the VP5 delta — `solution-offline-sync`.

| VP5 piece | Where |
| --- | --- |
| Dexie mutation queue (per-feature partitions, idempotency key, `touchedFields`) | libs/shared/offline-sync/src/lib/mutation-queue.{db,service}.ts (+ spec) |
| Replay orchestrator (concurrent partitions, FIFO within, server-wins `handleConflict` seam) | libs/shared/offline-sync/src/lib/replay-orchestrator.ts (+ spec) — also `MutationReplayRegistry`, `ReplayConflictError` |
| `provideOfflineSync()` (shell) / `provideFeatureReplay(factory)` (feature route) | libs/shared/offline-sync/src/lib/provide-offline-sync.ts |
| `notifications` slice (`show` / `dismiss` / `clearAll`, `selectNotifications`) | libs/shared/state/src/lib/notifications/ (+ spec), registered in store.config.ts |
| Facade enqueues a queueable op on `OfflineTransportError`, returns `{ queued: true }` | libs/orders/data-access/src/lib/orders.facade.ts (+ orders.facade.spec.ts) |
| Feature registers its replay handler in the route injector | libs/orders/feature/src/lib/orders.offline-sync.ts, wired in orders.routes.ts `providers` |
| Store tracks `pendingSync` + a `queued` status | libs/orders/feature/src/lib/orders.store.ts |
| `PendingSyncIndicatorComponent` (presentational, `count` input) | libs/shared/ui/src/lib/pending-sync-indicator/ (+ spec), mounted in order-form.component.ts |
| Shell wires the orchestrator | apps/platform-shell/src/app/app.config.ts (`provideOfflineSync()` only) |
| Offline-write e2e specs (written, not run here) | apps/platform-shell-e2e/src/offline.e2e.spec.ts |

## Running

    npm install
    npm test           # Vitest, jsdom — 20 files / 67 tests green (fake-indexeddb shims IndexedDB)
    npm run lint       # nx run-many -t lint — 11 projects green
    npm run build:prod # nx build platform-shell --configuration=production
    npm run build:sw   # nx build-sw platform-shell — generates dist/apps/platform-shell/browser/sw.js
    npx playwright install chromium
    npm run e2e        # apps/platform-shell-e2e

## Verified state

`npm test`, `npm run lint`, `nx build platform-shell --configuration=production`, and
`nx build-sw platform-shell` are all GREEN. Dexie is exercised under Vitest via `fake-indexeddb`
(the queue's "survives a page reload" spec opens a second connection to the same fake DB). Playwright
suites are written and configured but were not executed where this example was built.

## Fed back into the catalog

- `solution-offline-sync`'s Repository.extend tags `libs/shared/offline-sync` as `type:util`. That
  cannot hold — the lib reads the `connectivity`/`notifications` slices and is imported by feature
  Facades. It is `type:store` here, and the boundary allow-list gains `data-access → store` and
  `store → store`.
- The solution's `ReplayOrchestrator` sketch statically imports feature facades. That would either
  cycle or force feature code into the initial bundle. This example uses a `MutationReplayRegistry`
  the feature populates from its **route `providers`** (`provideFeatureReplay`); the shell only calls
  `provideOfflineSync()`. `nx lint` proved this — an earlier version that re-exported the feature's
  `provide*` through `@org/orders-feature` and imported it in `app.config.ts` failed with
  "Static imports of lazy-loaded libraries are forbidden".
- `solution-offline-sync` now carries `Implementation/GlobalStore/shared-state.project.extend.md` for
  the `notifications` slice — closing delta-conflict-analysis **Finding 4** (previously prose-only).
- `PendingSyncIndicatorComponent` (like `OfflineBannerComponent` before it) is presentational
  (`count` input) rather than store-injecting, to keep `libs/shared/ui` off `type:store`.
- Dexie (~65 kB) is in the initial bundle via `provideOfflineSync()` (the orchestrator must be live
  from bootstrap). The `initial` / `anyScript` budgets were bumped once, deliberately (500 kB warn /
  600 kB error), per the VP1 "adjust a budget only as a reviewed decision" rule.
