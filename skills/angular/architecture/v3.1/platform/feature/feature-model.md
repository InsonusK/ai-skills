# Feature Model — platform

A **federation system**: one host application that discovers and mounts independently built, independently deployed remote applications at runtime, plus the `@platform/contracts` package that is the only contract between them. Derived from the V1 `plateau-platform-monolith` (host) and `plateau-embeddable-app` (remote).

The **root product is `Platform`** — a deployment consisting of exactly one host build + zero-or-more remote builds + one `@platform/contracts` release. This model has two roles with their own variability:

- **Host** — the `apps/platform-shell` repository. The host **is a monolith**: `platform/`'s plateaus set `parent_plateaus` to the corresponding [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|`monolith/`]] plateaus, so every monolith feature (routing, state, `BackendDataAccess`, `Authentication`, `OfflineReadResilience`, …) is available to the host by composition and is **not re-modeled here**. This model adds only the federation delta.
- **Remote** — each embeddable-app repository. A remote is **not required to be a monolith** (not required to use Nx at all); it only satisfies `FederationRemoteContract`. Each remote is configured independently — the remote-side features are answered once per remote.

Built per [[skills/dotnet/architecture/v3.1/design/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]. See [[skills/angular/architecture/v3.1/README.md|the catalog overview]].

## The common baseline this model assumes (concretely)

### Host baseline

Everything the [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md#the-common-baseline-this-model-assumes-concretely|`monolith/` baseline]] is, **plus**:

```
apps/platform-shell/
  federation.config.js                         (Native Federation host: shared singletons, no static remotes)
  src/app/remote-registry.service.ts            (runtime resolver for remote remoteEntry URLs, from a manifest)
package.json
  @angular-architects/native-federation
  @platform/contracts                           (singleton: true, strictVersion: true)
```

`apps/platform-shell` is tagged `type:host`. It still contains no HTTP call, no business state, no feature component — mounting a remote is one manifest-resolved `loadChildren`-shaped entry in `app.routes.ts`, exactly like mounting a local feature.

### `@platform/contracts` baseline

Its own repository, published like an external package (semver): the `EventBus` / shared-state contract and the `SessionContract` *shape* (`currentUser`, `permissions`, `isAuthenticated`). No implementation of session logic — that is the host's `SessionSharing` feature.

### Remote baseline

Any workspace tool (Nx not required):

```
federation.config.js                            (remote: name, exposes: { './Module': ... }, remoteEntry)
package.json
  @angular-architects/native-federation
  @platform/contracts        (singleton: true, strictVersion: true)
src/
  the exposed module                            (mounts its own feature root segments — hierarchical routing one level down,
                                                 never hardcoding its own mount prefix)
```

No prescribed internal structure, state tier, or test setup — those are the remote team's choice unless `RemoteInternalArchitecture` is adopted.

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

`Host` and `Remote` are two sub-blocks of the same product, not an `Alternative` — a platform deployment always has a host and may have any number of remotes. The two dotted cross-catalog `Requires` edges point into `monolith/`: `SessionSharing` requires the host monolith has `Authentication` (it publishes that session), and `FederatedReadResilience` requires the host monolith has `OfflineReadResilience` (it adds one rule to that service worker). `RemoteSessionConsumption` is drawn with a "meaningful only if" edge to `SessionSharing` — a remote can *declare* session consumption with no host `SessionSharing`, it just always reads `isAuthenticated: false`.

## Features

| Name | Description | IsCommon | Role |
| --- | --- | --- | --- |
| RuntimeRemoteFederation | `apps/platform-shell` is a Native Federation **dynamic** host: which remotes exist and where they are served from is resolved from a runtime manifest (`RemoteRegistryService`), never compiled in. A remote is mounted via `loadRemoteModule` exactly like a directly-owned feature — one root-segment entry per remote. Angular and `@platform/contracts` are shared `singleton: true`. A failed remote load degrades to a fallback slot in that route, never a shell-wide crash. | true | host |
| PlatformContracts | `@platform/contracts` as an independently versioned, independently published package (its own repo): the `EventBus`/shared-state contract and the `SessionContract` shape. Cross-team compatibility is a semver contract, not a monorepo detail. Consumed as a strict `singleton: true` shared dependency by the host and every remote. | true | host + remote |
| FederationRemoteContract | What a remote must satisfy to be loadable: a valid `remoteEntry`, an exposed module, `singleton: true` on Angular + `@platform/contracts`, hierarchical route ownership inside the exposed module (never hardcoding its own mount prefix), an independent CI/CD pipeline, and **no import of `platform-shell` internals in either direction** — the only contract is `@platform/contracts` + the federation boundary. | true | remote |
| HostDesignSystemConsumption | The host declares the `design-system` package as a version-negotiated federation singleton (`singleton: true`, `strictVersion: false`) — upgrading the plain npm dependency the monolith already had — and applies the theme once at the document root in production; mounted remotes inherit it via the shared document. | false | host |
| SessionSharing | The host publishes a live `SessionContract` implementation through `@platform/contracts` so every mounted remote reads the same session instance the host reads. Session expiry propagates to every remote simultaneously with no polling. | false | host |
| FederatedReadResilience | The host monolith's Workbox service worker gains a fifth rule — stale-while-revalidate for federated remote chunks, with known remote origins sourced from `RemoteRegistryService`'s manifest — so a temporarily unreachable remote still mounts from its last-cached version. | false | host |
| RemoteSessionConsumption | The remote reads `SessionContract` (`currentUser`, `permissions`, `isAuthenticated`) from `@platform/contracts` — the same singleton the host published — and never implements its own login flow or keeps its own session copy. If `isAuthenticated` is false it renders a "not authenticated" state and defers to the host. Authorization checks are permission strings, never role names. | false | remote (per remote) |
| RemoteDesignSystemConsumption | The remote declares `design-system` as a version-negotiated federation singleton with an accurate `requiredVersion` range: it shares the host's already-loaded instance when ranges align, and falls back to its own bundled copy when they don't — never blocking its own deploy. The theme is imported only for standalone local development. | false | remote (per remote) |
| RemoteInternalArchitecture | The remote reuses the `monolith/` catalog's own internal feature models (`NxWorkspaceStructure`, `StateTieringPolicy`, `BackendDataAccess`, `SignalForms`, testing, …) inside its own repo, so the remote is a full monolith wrapped in a federation entry point. | false (aspirational) | remote (per remote) |

### Deliberately not rows

- **The monolith the host composes** — every host feature that is really a monolith feature (routing, state, data access, auth, offline) is a row in [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|`monolith/`]], not duplicated here.
- **`SessionContract` the type** is part of `PlatformContracts` (its shape); *implementing and publishing it* is `SessionSharing`; *reading it* is `RemoteSessionConsumption`.
- **`RemoteRegistryService`** is baseline structure of `RuntimeRemoteFederation`, not a feature.

## Aspirational candidates (owner asked to consider)

| Candidate | Rationale | Shape |
| --- | --- | --- |
| RemoteInternalArchitecture | Already a row above (marked aspirational). `solution-platform-embeddability` explicitly permits it: a remote "is free to apply solution #1's structure internally if it also chooses Nx." No V1 solution realizes the composition. | optional, per remote |
| ContractEventBus | `@platform/contracts` mentions an `EventBus` for host↔remote events, but no V1 solution details it (only `SessionContract` is worked out). A future feature: typed event channels between host and remotes. | optional, host + remote |
| RemoteHealthAndVersioning | Runtime health/compat surface for remotes (which remote is up, on which `@platform/contracts` major) beyond the load-time fallback. | optional, host |

## Open questions on V1

1. **Three V1 solutions are two-sided and must be split** during delta-conflict-detection:
   - `solution-platform-embeddability` → `solution-federation-host` (host: `RuntimeRemoteFederation`, `FederatedReadResilience`) + `solution-platform-contracts` (the package) + `solution-federation-remote` (remote: `FederationRemoteContract`).
   - `solution-design-system-application` → host half (`HostDesignSystemConsumption`) + remote half (`RemoteDesignSystemConsumption`).
   - `solution-authentication` → stays in `monolith/` as full auth; a new `solution-session-sharing` (host: `SessionSharing`) + `solution-session-consumption` (remote: `RemoteSessionConsumption`) live here.
   **Working hypothesis: all three split.**
2. **`plateau-embeddable-app`'s `parent_plateau: plateau-platform-monolith` is wrong** for a separate-repo product. **Working hypothesis: the v3.1 remote plateau(s) are built from scratch (`parent_plateaus` empty)**; the host↔remote relationship is a cross-catalog `Requires`, not `parent_plateaus`.
3. **Does `RuntimeRemoteFederation` require `HostDesignSystemConsumption`?** V1 `solution-design-system-application` `depends_on solution-platform-embeddability` (real), and every V1 platform plateau has both. But a federation host that mounts remotes which each bring their own UI, with no shared design system, is conceivable. **Working hypothesis: `HostDesignSystemConsumption` is variable (near-universal)** — a platform almost always wants one visual language, but it is not a federation prerequisite.
4. **V1 `solution-platform-embeddability` `depends_on solution-offline-first`** — over-strong (see [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md#open-questions-on-v1|monolith open questions]]). Here it becomes `FederatedReadResilience` requiring the host monolith's `OfflineReadResilience` — a real constraint **only for that one feature**, not for `RuntimeRemoteFederation`.
5. **Is a platform with zero remotes a valid `Platform`?** Structurally yes (a host ready to mount remotes, none registered yet). **Working hypothesis: yes** — `Remote` is `Optional (zero or more)`, and a zero-remote platform is the initial state of every platform, not a separate product.

## Out of scope

- **The host's monolith features are in `monolith/`.** This model is only the federation delta.
- **A remote's internal architecture** is unconstrained unless `RemoteInternalArchitecture` is adopted — this is a contract-conformance model for the remote role, not a build guide.
- **`design-system` is a separate catalog** — both `HostDesignSystemConsumption` and `RemoteDesignSystemConsumption` consume its published package; they do not model it.
- **Cross-catalog constraints** (`SessionSharing` → monolith `Authentication`, `FederatedReadResilience` → monolith `OfflineReadResilience`) are expressed as `Requires` into `monolith/`, and will appear in the [[skills/angular/architecture/v3.1/platform/variability-map.md|platform Variability Map]]'s Constraint column referencing monolith VP IDs.
- **`IsCommon` is a judgment call** — three features common (the federation mechanism, the contracts package, the remote contract); everything role-specific is variable.
