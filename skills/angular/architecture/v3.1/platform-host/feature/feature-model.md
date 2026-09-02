# Feature Model — platform-host

The **host** side of a federation system: a `monolith` application whose shell (`apps/platform-shell`) is turned into a Native Federation dynamic host that discovers and mounts independently deployed [[skills/angular/architecture/v3.1/embeddable-app/feature/feature-model.md|`embeddable-app`]] remotes at runtime, plus the `@platform/contracts` package that is the only contract between them. Derived from the V1 `plateau-platform-monolith` (the host half of `plateau-platform-embeddability` / `plateau-design-system-application` / `plateau-authentication`).

The **root product is `PlatformHost`** — one per platform (a platform has exactly one host). A `PlatformHost` **is a monolith**: this catalog's plateaus set `parent_plateaus` to the corresponding [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|`monolith/`]] plateaus, so every monolith feature is available by composition and is **not re-modeled here**. This model adds only the federation delta.

Built per [[skills/dotnet/architecture/v3.1/design/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]. See [[skills/angular/architecture/v3.1/README.md|the catalog overview]].

## Composed from `monolith/` (by `parent_plateaus`)

A `PlatformHost` inherits the full `monolith/` feature space — nothing below re-decides these:

| From `monolith/` | How the host uses it |
| --- | --- |
| NxWorkspaceStructure, HierarchicalRouting, StateTieringPolicy, SignalForms, ConsoleLogging, BusinessLayerTesting, ComponentTesting | unchanged — the host is a normal monolith |
| `GlobalStore` | the host's own cross-cutting state; also where `SessionSharing`'s session state and `Authentication`'s `auth` slice live |
| `BackendDataAccess` | the host's own features still talk to their backend |
| `Authentication` | **`SessionSharing` requires it** — the host authenticates, then publishes the session |
| `OfflineReadResilience` | **`FederatedReadResilience` requires it** — it adds one rule to that service worker |
| `PerformanceTunedRouting`, `OfflineWriteQueue`, `BackendLogDelivery` | available, independent of federation |

So a host plateau's full VP answer set = (its parent monolith plateau's answers) + (this model's rows below).

## The common baseline this model adds (concretely)

On top of the composed `monolith/` baseline:

```
apps/platform-shell/
  federation.config.js                         (Native Federation host: shared singletons, NO static remotes)
  src/app/remote-registry.service.ts            (runtime resolver: remote remoteEntry URLs from a manifest)
package.json
  @angular-architects/native-federation
  @platform/contracts                           (singleton: true, strictVersion: true)

@platform/contracts/                            (its OWN repository — published like an external package, semver)
  the EventBus / shared-state contract
  the SessionContract shape (currentUser, permissions, isAuthenticated)
```

`apps/platform-shell` is tagged `type:host`. It still contains no HTTP call, no business state, no feature component — mounting a remote is one manifest-resolved `loadChildren`-shaped entry in `app.routes.ts`, exactly like mounting a local feature. `@platform/contracts` carries **only contract shapes**, no session logic (that is `SessionSharing`).

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

Two cross-catalog `Requires` edges point into `monolith/`: `SessionSharing` requires the host has monolith `Authentication` (it publishes that session), and `FederatedReadResilience` requires the host has monolith `OfflineReadResilience` (it adds one rule to that service worker).

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| RuntimeRemoteFederation | `apps/platform-shell` is a Native Federation **dynamic** host: which remotes exist and where they are served from is resolved from a runtime manifest (`RemoteRegistryService`), never compiled in. A remote is mounted via `loadRemoteModule` exactly like a directly-owned feature — one root-segment entry per remote. Angular and `@platform/contracts` shared `singleton: true`. A failed remote load degrades to a fallback slot in that route, never a shell-wide crash. | true |
| PlatformContracts | `@platform/contracts` as an independently versioned, independently published package (its own repo): the `EventBus`/shared-state contract and the `SessionContract` shape. Cross-team compatibility is a semver contract, not a monorepo detail. Consumed as a strict `singleton: true` shared dependency by the host and every remote. | true |
| HostDesignSystemConsumption | The host declares the `design-system` package as a version-negotiated federation singleton (`singleton: true`, `strictVersion: false`) — upgrading the plain npm dependency the monolith already had — and applies the theme once at the document root in production; mounted remotes inherit it via the shared document. | false |
| SessionSharing | The host publishes a live `SessionContract` implementation through `@platform/contracts` so every mounted remote reads the same session instance the host reads. Session expiry propagates to every remote simultaneously with no polling. Requires monolith `Authentication`. | false |
| FederatedReadResilience | The host monolith's Workbox service worker gains a fifth rule — stale-while-revalidate for federated remote chunks, with known remote origins sourced from `RemoteRegistryService`'s manifest — so a temporarily unreachable remote still mounts from its last-cached version. Requires monolith `OfflineReadResilience`. | false |

### Deliberately not rows

- **Everything the host inherits from `monolith/`** — see [the composition table](#composed-from-monolith-by-parent_plateaus). Not duplicated as rows.
- **`SessionContract` the type** is part of `PlatformContracts` (its shape); *implementing and publishing it* is `SessionSharing`; *reading it* is `embeddable-app`'s `RemoteSessionConsumption`.
- **`RemoteRegistryService`** is baseline structure of `RuntimeRemoteFederation`, not a feature.

## Aspirational candidates (owner asked to consider)

| Candidate | Rationale | Shape |
| --- | --- | --- |
| ContractEventBus | `@platform/contracts` mentions an `EventBus` for host↔remote events, but no V1 solution details it (only `SessionContract` is worked out). A future feature: typed event channels between host and remotes. | optional, host + remote |
| RemoteHealthAndVersioning | A runtime health/compat surface for remotes (which remote is up, on which `@platform/contracts` major) beyond the load-time fallback. | optional, host |

The `monolith/` aspirational candidates (SSR, i18n, telemetry, feature-flags, runtime-config, `PersistedState`) apply to the host **via composition** — they belong to `monolith/`, not here.

## Open questions on V1

1. **`solution-platform-embeddability` is three things in one solution.** **Working hypothesis: split during delta-conflict-detection** into `solution-federation-host` (this catalog: `RuntimeRemoteFederation`, `FederatedReadResilience`), `solution-platform-contracts` (this catalog: `PlatformContracts` — the package), and `solution-federation-remote` (the `embeddable-app` catalog: its `FederationRemoteContract`).
2. **`solution-design-system-application` is two-sided.** **Working hypothesis: split** into `solution-host-design-system-consumption` (this catalog: `HostDesignSystemConsumption`) and `solution-remote-design-system-consumption` (the `embeddable-app` catalog).
3. **`solution-authentication`'s `SessionContract` publication is a separate concern.** Full auth stays in `monolith/`. **Working hypothesis: a new `solution-session-sharing`** (this catalog: `SessionSharing`) `depends_on` the monolith `solution-authentication`.
4. **Does `RuntimeRemoteFederation` require `HostDesignSystemConsumption`?** V1 `solution-design-system-application` `depends_on solution-platform-embeddability` (real), and every V1 platform plateau has both. But a federation host whose remotes each bring their own UI, with no shared design system, is conceivable. **Working hypothesis: `HostDesignSystemConsumption` is variable (near-universal)** — a platform almost always wants one visual language, but it is not a federation prerequisite.
5. **V1 `solution-platform-embeddability` `depends_on solution-offline-first`** — over-strong (see [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md#open-questions-on-v1|monolith open questions]]). **Working hypothesis:** only `FederatedReadResilience` requires the host's monolith `OfflineReadResilience`; `RuntimeRemoteFederation` does not.
6. **Is a host with zero remotes a valid `PlatformHost`?** Structurally yes — a host ready to mount remotes, none registered yet, is the initial state of every platform. **Working hypothesis: yes.**

## Out of scope

- **The host's monolith features are in `monolith/`.** This model is only the federation delta.
- **The remote side** — what an embeddable app must satisfy, and its own variability — is in [[skills/angular/architecture/v3.1/embeddable-app/feature/feature-model.md|`embeddable-app/`]].
- **`design-system` is a separate catalog** — `HostDesignSystemConsumption` consumes its published package.
- **Cross-catalog constraints** (`SessionSharing` → monolith `Authentication`, `FederatedReadResilience` → monolith `OfflineReadResilience`) appear in the [[skills/angular/architecture/v3.1/platform-host/variability-map.md|platform-host Variability Map]]'s Constraint column referencing monolith VP IDs.
- **`IsCommon` is a judgment call** — two features common (the federation mechanism, the contracts package); everything else is a variable host upgrade.
