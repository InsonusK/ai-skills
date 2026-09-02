---
tags:
  - concern/architecture
  - stack/typescript
---

# platform-host Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md|platform-host/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith]], [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|embeddable-app]], [[skills/angular/architecture/v3.1/design-system/variability-map.md|design-system]].

**Status.** `v3.1/solutions/` holds the migrated + split + new solutions (Stage 3). **Realized by** links point into it.

**A `platform-host` composes a `monolith`.** Every [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith VP]] (VP1–VP8 there) is also answered by a `platform-host` plateau, via `parent_plateaus`. This map covers **only the federation delta** — three VPs on top of whatever monolith plateau the host builds on. Constraint entries that reference `monolith:VPn` mean "the monolith plateau this host composes must have that answer".

## Variation Points

Common baseline (`RuntimeRemoteFederation`, `PlatformContracts`) is not a row — every `platform-host` has both. See [Features that are not VPs](#features-that-are-not-vps).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **HostDesignSystemConsumption** — does the host consume `design-system` as a version-negotiated federation singleton and apply the theme once at the document root? | Yes / No | — | Yes → the host half of [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md\|solution-host-design-system-consumption]] → v3.1 `solution-host-design-system-consumption` (split) | Cross-catalog: consumes the [[skills/angular/architecture/v3.1/design-system/variability-map.md\|design-system]] published package; version-negotiates with each remote's `RemoteDesignSystemConsumption` | No |
| VP2 | **SessionSharing** — does the host publish a live `SessionContract` through `@platform/contracts` for remotes to read? | Yes / No | **requires `monolith:VP7` (Authentication) = Yes** | Yes → new `solution-session-sharing` (Stage 3, carved from V1 [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md\|solution-authentication]]'s `SessionContract` part) — `depends_on` the monolith `solution-authentication` | Cross-catalog: `embeddable-app`'s `RemoteSessionConsumption` reads what this publishes; wires the `auth` slice (monolith `GlobalStore`) into `@platform/contracts` (`PlatformContracts`) | No |
| VP3 | **FederatedReadResilience** — does the host's service worker gain a fifth rule (stale-while-revalidate for federated remote chunks)? | Yes / No | **requires `monolith:VP4` (OfflineReadResilience) = Yes** | Yes → the service-worker-extension part of V1 [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md\|solution-federation-host]] → v3.1 folded into `solution-federation-host` (or a thin `solution-federated-read-resilience` extension) | Cross-feature: extends the monolith `solution-offline-first` service worker; sources known remote origins from `RemoteRegistryService` (`RuntimeRemoteFederation`) | No |

### VP2's constraint — SessionSharing requires monolith Authentication

The host can only publish a session it has. V1 `solution-authentication` `depends_on solution-platform-embeddability` — the wrong direction and over-bundled (feature-model open question 3). v3.1 inverts it: full auth lives in `monolith/` with no federation dependency; `solution-session-sharing` (this catalog) `depends_on` both the monolith `solution-authentication` and `solution-platform-contracts`. Recorded as a v3.1 ADR at Stage 3.

### VP3's constraint — FederatedReadResilience requires monolith OfflineReadResilience

V1 `solution-platform-embeddability` `depends_on solution-offline-first` unconditionally, but its own prose says the fifth caching rule applies only "if the Offline-first solution is also present" (feature-model open question 5). v3.1: `RuntimeRemoteFederation` has **no** dependency on offline; only `FederatedReadResilience` requires the host's monolith `OfflineReadResilience`, and it is a separate, optional host feature.

## Features that are not VPs

- **`RuntimeRemoteFederation`** and **`PlatformContracts`** — every `platform-host` has both by definition (that is what makes it a host). Shared core here, not variability. Realized by the host + contracts halves of the split V1 `solution-platform-embeddability` → v3.1 `solution-federation-host` + `solution-platform-contracts`.
- **The monolith the host composes** — all of `monolith/`'s VPs are answered by the host's plateau, but they are rows in [[skills/angular/architecture/v3.1/monolith/variability-map.md|the monolith map]], not duplicated here.
- **Aspirational**: `ContractEventBus` (typed host↔remote event channels — `@platform/contracts` mentions an `EventBus`, no V1 solution details it) and `RemoteHealthAndVersioning`.

## Plateau Map derivation

**No plateaus exist in `v3.1/platform-host/` yet.**

### Reference: V1 → v3.1

| V1 plateau | v3.1 platform-host plateau | composes monolith plateau | + platform-host VPs |
| --- | --- | --- | --- |
| `plateau-platform-monolith` | e.g. `plateau-federation-host` | `plateau-offline-monolith` (monolith VP1–VP5) | VP1=Yes (`HostDesignSystemConsumption`); VP2/VP3 = No |
| *(V1 `plateau-monitored-app` = platform-monolith + `logging-global`)* | a deeper host plateau | `plateau-monitored-app` (monolith, adds VP6) | VP1=Yes |
| *(V1 `plateau-multiuser-app` = monitored-app + `authentication`)* | the full host plateau | `plateau-multiuser-app` (monolith, adds VP7) | VP1=Yes, **VP2=Yes** (`SessionSharing` — now satisfiable, monolith VP7=Yes) |

V1 bundled "become a platform" with "add offline write queue" (`platform-monolith` descends from `offline-monolith`). v3.1 does not force that: a `platform-host` can compose *any* monolith plateau — `plateau-online-monolith` included — so a federation host with no offline support at all is a valid v3.1 plateau V1 has no equivalent for.

### Combinations v3.1 allows that V1 has no plateau for

- **Host composing `plateau-online-monolith`** — a federation host with no preloading tuning, no offline, no auth. V1's platform chain always descends from `offline-monolith`.
- **VP1=No** — a host whose remotes each ship their own visual language, no shared design system.
- **VP2=Yes, VP3=No** — session-sharing platform without federated read resilience (host has monolith auth but not monolith offline).

## Out of scope

- **Realized-by links are provisional** (Stage 3 repoints).
- **The monolith VPs** are in [[skills/angular/architecture/v3.1/monolith/variability-map.md|the monolith map]]; this map is the federation delta only.
- **The remote side** (`FederationRemoteContract`, `RemoteSessionConsumption`, `RemoteDesignSystemConsumption`) is [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|the embeddable-app map]].
- **`Migration = No`** everywhere here — becoming a federation host, or adding session sharing, has no observed post-deployment transition in V1 (V1's platform chain is a design-time progression that also carries the monolith transitions; those are marked `Yes` in the monolith map).
