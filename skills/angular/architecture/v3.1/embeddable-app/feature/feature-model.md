# Feature Model — embeddable-app

The **remote** side of a federation system: an independently built, independently deployed application, in its own repository, owned and released by its own team, that a [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md|`platform-host`]] loads at runtime. Derived from the V1 `plateau-embeddable-app` (the remote half of `plateau-platform-embeddability` / `plateau-design-system-application` / `plateau-authentication`).

The **root product is `EmbeddableApp`**. Any independently deployed application in this architecture — regardless of which team builds it or with what tooling — must conform to this catalog's baseline to be loadable by a `platform-host`. Each remote is configured **independently**; a platform deployment has zero or more of them, and this model describes one.

Built per [[skills/dotnet/architecture/v3.1/design/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]]. See [[skills/angular/architecture/v3.1/README.md|the catalog overview]].

## The common baseline this model assumes (concretely)

The embeddable-app repository is **not required to adopt Nx or any platform tooling** — only the federation contract is fixed. This is a deliberately minimal baseline (it is the opposite end of the spectrum from `monolith/`):

```
federation.config.js / native-federation config      (remote: name, exposes: { './Module': ... }, remoteEntry)
package.json
  @angular-architects/native-federation
  @platform/contracts        (singleton: true, strictVersion: true — from the platform-host catalog)
src/
  bootstrap.ts / the exposed module                   (mounts its own feature root segments — hierarchical routing
                                                        one level down, NEVER hardcoding its own mount prefix)
(its own CI/CD pipeline, independent of the platform's release schedule)
```

There is no prescribed `feature`/`data-access` split, no required state tier, no required test setup, no required design system — those are the remote team's choice unless `RemoteInternalArchitecture` is adopted. The contract is exactly: a valid `remoteEntry`, an exposed module, `singleton: true` on Angular + `@platform/contracts`, hierarchical route ownership inside the exposed module, and no import of `platform-shell` internals in either direction.

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

`RemoteSessionConsumption` and `RemoteDesignSystemConsumption` have dotted edges to the `platform-host` catalog: a remote can *declare* session consumption with no host `SessionSharing` (it just always reads `isAuthenticated: false`), and its design-system version negotiation is *with* the host's `HostDesignSystemConsumption`. Neither is a hard `Requires` on the host — a remote is built and deployed without knowing which host will load it.

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| FederationRemoteContract | What a remote must satisfy to be loadable: a valid Native Federation `remoteEntry` and an exposed module; `singleton: true` on Angular + `@platform/contracts` so host and remote share one runtime and one contract instance; hierarchical route ownership inside the exposed module (mounts its own feature root segments one level down, never hardcoding its own mount prefix); an independent CI/CD pipeline; and **no import of `platform-shell` internals in either direction** — the only contract is `@platform/contracts` + the federation boundary. | true |
| RemoteSessionConsumption | The remote reads `SessionContract` (`currentUser`, `permissions`, `isAuthenticated`) from `@platform/contracts` — the same singleton the host published — and never implements its own login flow or keeps its own session copy. If `isAuthenticated` is false it renders a "not authenticated" state and defers to the host. Authorization checks, where the remote makes them, are permission strings, never role names. | false |
| RemoteDesignSystemConsumption | The remote declares `design-system` as a version-negotiated federation singleton (`singleton: true`, `strictVersion: false`) with an accurate `requiredVersion` range: it shares the host's already-loaded instance when ranges align, and falls back to its own bundled copy when they don't — never blocking its own deploy. The theme is imported only for standalone local development; in production its mounted components inherit the host shell's theme from the shared document. | false |
| RemoteInternalArchitecture | The remote reuses the [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md|`monolith/`]] catalog's own internal feature models (`NxWorkspaceStructure`, `StateTieringPolicy`, `GlobalStore`, `BackendDataAccess`, `SignalForms`, testing, …) inside its own repo — so the remote is a full monolith wrapped in a federation entry point. `solution-platform-embeddability` explicitly permits this ("free to apply solution #1's structure internally if it also chooses Nx"). | false (aspirational) |

### Deliberately not rows

- **`SessionContract` the type** is part of `platform-host`'s `PlatformContracts`; this model only *reads* it (`RemoteSessionConsumption`).
- **The exposed module / `remoteEntry`** is the baseline structure the family *is*, described above, not a feature.
- **The remote's own internal architecture** (if any) is `monolith/`'s feature space, reached via `RemoteInternalArchitecture` — not re-modeled here.

## Aspirational candidates (owner asked to consider)

| Candidate | Rationale | Shape |
| --- | --- | --- |
| RemoteInternalArchitecture | Already a row above (marked aspirational). No V1 solution realizes the composition of `monolith/` inside a remote. When built, this catalog's `RemoteInternalArchitecture=Yes` plateaus would `parent_plateaus` a `monolith/` plateau. | optional, whole-repo |
| ContractEventBus (remote side) | The remote half of `platform-host`'s aspirational `ContractEventBus` — typed event channels to/from the host. | optional, per remote |

## Open questions on V1

1. **`plateau-embeddable-app`'s `parent_plateau: plateau-platform-monolith` is wrong** for a separate-repo product with a different baseline. **Working hypothesis: this catalog's plateaus are built from scratch (`parent_plateaus` empty)**; the host↔remote relationship is expressed as cross-catalog references, not `parent_plateaus`. (`RemoteInternalArchitecture=Yes` plateaus are the exception — they `parent_plateaus` a `monolith/` plateau.)
2. **Are `RemoteSessionConsumption` and `RemoteDesignSystemConsumption` really optional?** V1's single `plateau-embeddable-app` has both. But a public widget with no user context could skip `SessionContract` entirely, and a remote that renders no shared-styled UI could skip the design system. **Working hypothesis: `FederationRemoteContract` is the only common feature; the other two are variable (near-universal).** This gives the catalog its first two real VPs. Owner call.
3. **Three V1 solutions are two-sided** (see [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md#open-questions-on-v1|platform-host open questions]] 1–3). This catalog's realizations are the remote halves: `solution-federation-remote` (`FederationRemoteContract`), `solution-remote-design-system-consumption` (`RemoteDesignSystemConsumption`), `solution-session-consumption` (`RemoteSessionConsumption`).
4. **Does this catalog need its own testing / structure solutions?** V1's `plateau-embeddable-app` includes none. **Working hypothesis: no** — testing and internal structure are the remote team's choice; only `RemoteInternalArchitecture=Yes` pulls in `monolith/`'s testing features.

## Out of scope

- **The host side** of federation, design-system sharing, and session publication is in [[skills/angular/architecture/v3.1/platform-host/feature/feature-model.md|`platform-host/`]].
- **`@platform/contracts`** is owned and published by `platform-host/` (`PlatformContracts`); this model consumes it.
- **The remote's own internal architecture** is unconstrained unless `RemoteInternalArchitecture` is adopted — this is a contract-conformance model, not a build guide.
- **`IsCommon` is a judgment call** — only `FederationRemoteContract` is common (pending open question 2); everything else is a per-remote choice.
