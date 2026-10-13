# plateau-embeddable-app — trivial remote

A **limited** example: a minimal Native Federation **remote** that satisfies the host
contract and nothing more. Built from scratch (`parent_plateaus: []`) — a remote is not a
continuation of the platform chain. It imposes no internal architecture; this one is
deliberately tiny.

## What's here

| Piece | Where |
| --- | --- |
| Native Federation **remote** (`ng add @angular-architects/native-federation --type remote`) exposing `./Routes` | `federation.config.mjs`, `src/main.ts` (`initFederation`) |
| `REMOTE_ROUTES` — the exposed module: this remote's own root-relative routes; the host assigns the mount segment, this remote never references it | `src/app/remote.routes.ts` |
| `requirePermission('...')` — a `CanActivateFn` reading `SESSION_CONTRACT` from `@platform/contracts`; NO login flow, NO local session copy | `src/app/session/require-permission.ts` (+ spec) |
| `*hasPermission` structural directive — same permission-string model as the host | `src/app/session/has-permission.directive.ts` |
| `ReportsComponent` — renders a not-authenticated state when `isAuthenticated()` is false (the host owns any redirect); gates a control on a permission string | `src/app/reports/reports.component.ts` (+ spec) |
| `@platform/contracts` + Angular `singleton: true, strictVersion: true`; `design-system` would be `strictVersion: false` with this team's real `requiredVersion` (commented) | `federation.config.mjs` |
| `@platform/contracts` — published by the platform-host team; vendored here as a tarball | `platform-contracts-1.0.0.tgz` |

## Running

    npm install
    npm test            # ng test — Vitest, 2 files / 6 tests (requirePermission + ReportsComponent)
    npm run build       # ng build — dist/embeddable-app/browser/remoteEntry.json exposes ./Routes, shares @platform/contracts

## Verified state

`npm test` (6) and `npm run build` (`remoteEntry.json` exposes `./Routes` and shares
`@platform/contracts` + Angular as strict singletons) are GREEN. The full host↔remote
`loadRemoteModule` round trip is exercised by the platform-host example's federation smoke
e2e (written, not run in the sandbox).

## Fed back into the catalog

- **The exposed module is a `Routes` array (`./Routes`), not a component.** `solution-federation-remote`'s
  `routes.ts.extend` has the remote mount its own features' root segments — the host's
  `loadChildren` reads `REMOTE_ROUTES` off the exposed module and Angular's router composes
  the full path once mounted. No mount prefix is baked into the remote.
- **`requirePermission` here is the remote's own tiny guard** reading `SESSION_CONTRACT` —
  it does NOT import `@org/shared-auth-ui` (that is a monolith lib). An unauthenticated
  session returns `false` and the component renders a not-authenticated state; the remote
  never navigates to a login route.
- Consuming `@platform/contracts` needs no `@angular/core` dep of the remote's own beyond
  what Angular already provides — the token + `Signal` types come through the peer.
