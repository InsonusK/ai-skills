# plateau-multiuser-monolith — reference example

The `plateau-offline-full-monolith` workspace, evolved one plateau further down the chain — **the
last plateau of the monolith chain and `plateau-platform-host`'s parent**. Everything from the
parent chain still applies (see those READMEs). This file records only the VP6 + VP7 delta:
`solution-logging-global` and `solution-authentication`.

## VP6 — BackendLogDelivery (`solution-logging-global`)

| VP6 piece | Where |
| --- | --- |
| `BackendLogSink` joins `ConsoleLogSink` on the `LOG_SINKS` multi-provider seam — no call-site changes | libs/shared/logging/src/lib/backend-log-sink.ts (+ spec) |
| Only `warn` / `error` / `report` forwarded; batched, flushed on a timer / size threshold; `navigator.sendBeacon` on `pagehide` | libs/shared/logging/src/lib/backend-log-sink.ts |
| `LogRetryQueue` — a bounded IndexedDB (Dexie) queue: count / age / size limits enforced independently, oldest-first eviction, retry stops on first failure per cycle | libs/shared/logging/src/lib/log-retry-queue.ts (+ spec) |
| `LogLevel` gains `'report'`; `LoggerService.report()` always reaches `BackendLogSink` regardless of `MIN_LOG_LEVEL` | libs/shared/logging/src/lib/{log-sink,logger.service,console-log-sink}.ts (+ logger.service.spec.ts) |
| `GlobalErrorHandler` — routes every uncaught exception through `LoggerService.error` with only `message` / `stack` | apps/platform-shell/src/app/global-error-handler.ts (+ spec), registered once in app.config.ts |

## VP7 — Authentication (`solution-authentication`)

| VP7 piece | Where |
| --- | --- |
| `auth` slice — classical NgRx `createFeature`: in-memory `accessToken`, `permissions` (strings), `refreshInProgress`; `selectIsLoggedIn` extra-selector | libs/shared/state/src/lib/auth/auth.{actions,reducer,effects,selectors}.ts (+ auth.spec.ts, auth.effects.spec.ts) |
| `AuthFacade` — `login` / `silentRefresh` / `logout` HTTP round trips via `BaseHttpService` | libs/shared/state/src/lib/auth/auth.facade.ts |
| `authInterceptor` — attaches `Authorization: Bearer <token>`, on 401 dispatches one `Silent Refresh Requested`, skips `/auth/` requests | libs/shared/state/src/lib/auth/auth.interceptor.ts (+ spec) |
| `libs/shared/auth-ui` (**new Nx project**, `type:store`) — `*hasPermission` directive, `requirePermission` guard factory, login form, forbidden page | libs/shared/auth-ui/src/lib/ (+ has-permission.directive.spec.ts, permission.guard.spec.ts) |
| Shell wiring — `withInterceptors([authInterceptor])`, `provideAppInitializer` dispatching one bootstrap `Silent Refresh Requested`, `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`, `/login` + `/forbidden` routes | apps/platform-shell/src/app/{app.config,app.routes}.ts |
| Feature attachment — `canActivate: [requirePermission('orders.archive')]` on a feature-owned route; `*hasPermission="'orders.delete'"` on a control | libs/orders/feature/src/lib/orders.routes.ts, .../order-form/order-form.component.ts |

## Running

    npm install
    npm test           # Vitest, jsdom — 28 files / 98 tests green (fake-indexeddb shims IndexedDB for the Dexie queues)
    npm run lint       # nx run-many -t lint — 12 projects green
    npm run build:prod # nx build platform-shell --configuration=production — initial 454 kB
    npm run build:sw   # nx build-sw platform-shell — sw.js precaches 9 files / 491 KiB
    npx playwright install chromium
    npm run e2e        # apps/platform-shell-e2e

## Verified state

`npm test`, `npm run lint`, `nx build platform-shell --configuration=production`, and
`nx build-sw platform-shell` are all GREEN. Both Dexie queues (`mutation-queue`, `log-retry-queue`)
are exercised under Vitest via `fake-indexeddb` — `libs/shared/logging/src/test-setup.ts` and the two
`libs/orders/*/src/test-setup.ts` files add `import 'fake-indexeddb/auto';`. Playwright suites are
written and configured but were not executed where this example was built.

## Fed back into the catalog

- **`libs/shared/auth-ui` is `type:store`, not `type:util`.** `solution-authentication`'s
  `Repository.extend` and `shared-auth-ui.project.create` tag it `type:util`. That cannot hold — the
  `*hasPermission` directive and the `requirePermission` guard both inject `Store` to read
  `selectPermissions`, and a `type:util` lib may not depend on `type:store` (`libs/shared/state`).
  It is `type:store` here.
- **`Login Succeeded` carries `{ user, accessToken, permissions }`.** The solution's
  `auth.store.ts.create` sketch gives it `{ user }` only, which leaves no way for a fresh login to
  populate the in-memory token — `Silent Refresh Succeeded` already carries both. The reducer treats
  the two the same.
- **`libs/shared/logging` → `libs/shared/http-core`.** VP6's `BackendLogSink` / `LogRetryQueue` send
  batches through the base HTTP service, so `type:util → type:data-access` is added to the boundary
  allow-list (kept to shared primitives by the `scope:shared → scope:shared` rule). `type:util`
  also gains `→ type:store` for symmetry with the store-connected shared libs.
- **`GlobalErrorHandler` lives in `apps/platform-shell`, not `libs/shared/logging`.** It is a
  composition-root concern (it *is* the app's `ErrorHandler`), and the solution's own
  `PlatformHost/global-error-handler.ts.create` places it there.
- **The bootstrap silent-refresh uses `provideAppInitializer`.** The solution says "an
  `APP_INITIALIZER`-equivalent or root route resolver"; `provideAppInitializer(() => inject(Store).dispatch(...))`
  is the Angular-22 form, dispatched exactly once in `app.config.ts`.
- **VP5 offline-sync — a per-entity `syncStatus` state machine, not a bare pending count.**
  `solution-offline-sync` (carried in from `plateau-offline-full-monolith`) only specified a
  `pendingSync` *count*. The feature store now carries `syncStatus` per row —
  `queued → sending → (cleared | failed | conflict)` — driven by two new `FeatureReplay` callbacks
  (`onReplayStart` / `onReplayResult`) the `ReplayOrchestrator` calls around each replay. The
  indicator's count is derived from the rows; `OrdersStore.hydratePending()` rebuilds the optimistic
  rows from the persisted Dexie queue on a cold start. `ui-status-badge`'s `status` input is
  loosened to `string` (presentational).
- Budgets unchanged from VP5 (`initial` 500 kB warn / 600 kB error; `anyScript` 460 / 600) — the
  `auth`/`logging` additions land in lazy chunks or stay within the existing initial headroom
  (measured initial 454 kB).
