---
tags:
  - concern/architecture
  - stack/typescript
---

# platform-host Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md|platform-host/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith]], [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|embeddable-app]], [[skills/angular/architecture/v3.1/design-system/variability-map.md|design-system]].

**Status.** `v3.1/solutions/` holds the migrated / split / new solutions; every **Realized by** cell links one of them. The plateau↔VP view lives in [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-repository.md|platform-host/plateau/plateau-repository.md]] ([[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]]).

**A `platform-host` composes a `monolith`.** Every [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith VP]] (VP1–VP8 there) is also answered by a `platform-host` plateau, via `parent_plateaus`. This map covers **only the federation delta** — three VPs on top of whatever monolith plateau the host builds on. Constraint entries that reference `monolith:VPn` mean "the monolith plateau this host composes must have that answer".

## Variation Points

Common baseline (`RuntimeRemoteFederation`, `PlatformContracts`) is not a row — every `platform-host` has both. See [Features that are not VPs](#features-that-are-not-vps).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **HostDesignSystemConsumption** — does the host consume `design-system` as a version-negotiated federation singleton and apply the theme once at the document root? | Yes / No | — | Yes → [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md\|solution-host-design-system-consumption]] (the host half; split from V1 `solution-design-system-application`) | Cross-catalog: consumes the [[skills/angular/architecture/v3.1/design-system/variability-map.md\|design-system]] published package; version-negotiates with each remote's `RemoteDesignSystemConsumption` | No |
| VP2 | **SessionSharing** — does the host publish a live `SessionContract` through `@platform/contracts` for remotes to read? | Yes / No | **requires `monolith:VP7` (Authentication) = Yes** | Yes → [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md\|solution-session-sharing]] (carved from V1 `solution-authentication`'s `SessionContract` part; `depends_on` the monolith [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md\|solution-authentication]]) | Cross-catalog: `embeddable-app`'s `RemoteSessionConsumption` reads what this publishes; wires the `auth` slice (monolith `GlobalStore`) into `@platform/contracts` (`PlatformContracts`) | No |
| VP3 | **FederatedReadResilience** — does the host's service worker gain a fifth rule (stale-while-revalidate for federated remote chunks)? | Yes / No | **requires `monolith:VP4` (OfflineReadResilience) = Yes** | Yes → [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md\|solution-federation-host]] (the conditional service-worker extension — the 5th SW rule, `Implementation/ServiceWorker/`) | Cross-feature: extends the monolith `solution-offline-first` service worker; sources known remote origins from `RemoteRegistryService` (`RuntimeRemoteFederation`) | No |

### VP2's constraint — SessionSharing requires monolith Authentication

The host can only publish a session it has. V1 `solution-authentication` `depends_on solution-platform-embeddability` — the wrong direction and over-bundled (feature-model open question 3). v3.1 inverts it: full auth lives in `monolith/` with no federation dependency; `solution-session-sharing` (this catalog) `depends_on` the monolith `solution-authentication`, `solution-platform-contracts`, and `solution-federation-host`. Recorded in [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/adr/session-contract-ownership.md|a v3.1 ADR]].

### VP3's constraint — FederatedReadResilience requires monolith OfflineReadResilience

V1 `solution-platform-embeddability` `depends_on solution-offline-first` unconditionally, but its own prose says the fifth caching rule applies only "if the Offline-first solution is also present" (feature-model open question 5). v3.1: `RuntimeRemoteFederation` has **no** dependency on offline; only `FederatedReadResilience` requires the host's monolith `OfflineReadResilience`, and it is a separate, optional host feature.

## Features that are not VPs

- **`RuntimeRemoteFederation`** and **`PlatformContracts`** — every `platform-host` has both by definition (that is what makes it a host). Shared core here, not variability. Realized by the host + contracts halves of the split V1 `solution-platform-embeddability` → v3.1 `solution-federation-host` + `solution-platform-contracts`.
- **The monolith the host composes** — all of `monolith/`'s VPs are answered by the host's plateau, but they are rows in [[skills/angular/architecture/v3.1/monolith/variability-map.md|the monolith map]], not duplicated here.
- **Aspirational**: `ContractEventBus` (typed host↔remote event channels — `@platform/contracts` mentions an `EventBus`, no V1 solution details it) and `RemoteHealthAndVersioning`.

## Out of scope

- **The plateau↔VP view** — the `plateau-platform-host` matrix, its cross-catalog lineage, and the V1 reference mapping — lives in [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-repository.md|plateau/plateau-repository.md]], not here ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map.md|ADR]]).
- **The monolith VPs** are in [[skills/angular/architecture/v3.1/monolith/variability-map.md|the monolith map]]; this map is the federation delta only.
- **The remote side** (`FederationRemoteContract`, `RemoteSessionConsumption`, `RemoteDesignSystemConsumption`) is [[skills/angular/architecture/v3.1/embeddable-app/variability-map.md|the embeddable-app map]].
- **`Migration = No`** everywhere here — becoming a federation host, or adding session sharing, has no observed post-deployment transition in V1 (V1's platform chain is a design-time progression that also carries the monolith transitions; those are marked `Yes` in the monolith map).
