# plateau-platform-host — federation smoke test

A **limited** example: not the full `plateau-multiuser-monolith` app, just the **federation
delta** on top of it — enough to prove the host wiring. Everything a host inherits from the
monolith (state, data-access, auth, offline, logging, the design system) lives in
`plateau-multiuser-monolith`; this example demonstrates only what `solution-federation-host` +
`solution-platform-contracts` + `solution-session-sharing` + `solution-host-design-system-consumption`
add.

## What's here

| Piece | Where |
| --- | --- |
| Native Federation **dynamic host** (`ng add @angular-architects/native-federation --type dynamic-host`) | `federation.config.mjs`, `src/main.ts` (`initFederation`) |
| `@platform/contracts` — its own package (types + DI tokens only, no impl); the ONE build-time contract, a strict federation singleton | `platform-contracts/` (source) + `platform-contracts-1.0.0.tgz` (vendored — stands in for the published package) |
| `RemoteRegistryService` — resolves remotes from a **runtime** manifest, `loadRemote()` rejects (never throws at bootstrap); a failed load degrades to a fallback slot | `src/app/remote-registry/remote-registry.service.ts` (+ spec) |
| `HostSession` — the host is the **only** provider of `SESSION_CONTRACT` (`solution-session-sharing`); a read-only signal view of what the real `auth` slice holds | `src/app/session/host-session.ts` (+ spec) |
| The host mounts a remote at one root segment via `loadChildren` → `RemoteRegistryService.loadRemote('embeddable-app')` → its `REMOTE_ROUTES` | `src/app/app.routes.ts` |
| `@platform/contracts` + Angular `singleton: true, strictVersion: true`; `design-system` would be `strictVersion: false` (commented) | `federation.config.mjs` |
| Federation smoke e2e (host loads remote, shares one session, fallback on failure) | `e2e/federation-smoke.e2e.spec.ts` — written, not run here |

## Running

    # (re)build & pack @platform/contracts — already vendored as a tarball:
    (cd platform-contracts && npm install && npm run pack)

    npm install
    npm test            # ng test — Vitest, 2 files / 6 tests (RemoteRegistryService + HostSession/SESSION_CONTRACT)
    npm run build       # ng build — Native Federation host bundle
    npx tsc -p tsconfig.e2e.json   # typecheck the Playwright smoke spec

    # federation smoke test (needs the remote too):
    (cd ../embeddable-app && npx ng serve --port 4401) &
    npx ng serve --port 4400 &
    npx playwright test

## Verified state

`npm test` (6), `npm run build` (Native Federation host — `dist/platform-host/browser/remoteEntry.json`
shares `@platform/contracts` + Angular as strict singletons), and `tsc -p tsconfig.e2e.json`
are GREEN. The two-server Playwright smoke test is written and configured but not executed
where this example was built.

## Fed back into the catalog

- **`@angular-architects/native-federation@22.1.2` supports Angular 22's `@angular/build`.**
  `ng add … --type dynamic-host` switches the build to `@angular-architects/native-federation:build`
  and keeps the test target on `@angular/build:unit-test` (with an `esbuild` buildTarget). No
  V1 `@angular-architects/module-federation` / webpack — Native Federation + Dynamic Federation
  is the mechanism.
- **`@platform/contracts` must ship ESM with explicit `.js` import extensions**
  (`export … from './session-contract.js'`) and a `rootDir` — a bare `moduleResolution: bundler`
  build emits extensionless specifiers Node/Vite refuse. It carries `@angular/core` as a **peer
  only**; a `file:`/symlink install nests a second `@angular/core` and breaks `Signal` type
  identity, so it is consumed as a packed **tarball**, not a workspace symlink.
- **`RemoteRegistryService.loadRemote` returns the exposed module** and the host route reads
  `REMOTE_ROUTES` off it — the remote exposes `./Routes` (a `Routes` array), not a component,
  so hierarchical route ownership carries one level down unchanged.
- The 5th service-worker rule (`FederatedReadResilience`, VP3) applies only where the parent
  monolith has `OfflineReadResilience` — it lives in `solution-federation-host`'s
  `ServiceWorker/service-worker.ts.extend`, conditional, recorded in this plateau's `registry/`.
