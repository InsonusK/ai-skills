# Feature Model — embeddable-app

An independently built, independently deployed application, in its own repository, owned and released by its own team, that the `platform-app` host loads at runtime. Derived from the V1 `plateau-embeddable-app` (3 solutions: `platform-embeddability`, `design-system-application`, `authentication` — each on its *remote/consumer* side).

The **root product is `EmbeddableApp`**. Any independently deployed application in this architecture — regardless of which team builds it — must conform to this family's baseline to be loadable by the platform host. Like `design-system`, the V1 catalog has this family as a **single plateau with no Variation Points**; one aspirational candidate is proposed.

## The common baseline this model assumes (concretely)

The embeddable-app repository is **not required to adopt Nx or any platform tooling** — only the federation contract is fixed:

```
federation.config.js / native-federation config     (remote: name, exposes: { './Module': ... }, remoteEntry)
package.json
  @angular-architects/native-federation
  @platform/contracts        (singleton: true, strictVersion: true)
  <design-system package>    (singleton: true, strictVersion: false — version-negotiated)
src/
  bootstrap.ts / the exposed module                  (mounts its own feature root segments, hierarchical routing one level down)
  styles.scss                                        (imports the design-system theme — for standalone dev only; redundant once mounted)
(its own CI/CD pipeline, independent of the platform's release schedule)
```

There is no prescribed `feature`/`data-access` split, no required state-management tier, no required test setup — those are the embeddable team's own choice. The contract is: a valid `remoteEntry`, an exposed module, the shared singletons, hierarchical route ownership inside the exposed module, and session read via `SessionContract`.

## Feature diagram

@import "./diagrams/feature-diagram.mmd" {as="mermaid"}

No `Requires` edges — the three baseline features are independent obligations, all mandatory. `InternalArchitectureAdoption` is aspirational.

## Features

| Name | Description | IsCommon |
| --- | --- | --- |
| FederationRemoteContract | The app exposes a Native Federation `remoteEntry` and an exposed module; Angular and `@platform/contracts` are declared `singleton: true` so host and remote share one runtime and one contract instance; the exposed module mounts its own feature root segments using the platform's hierarchical route-ownership pattern one level down (never hardcoding its own mount prefix); the only contract with the platform is `@platform/contracts` plus the federation boundary — no import of platform-shell internals in either direction. | true |
| DesignSystemFederationConsumption | The `design-system` package declared as a version-negotiated federation singleton (`singleton: true`, `strictVersion: false`) with an accurate `requiredVersion` range: the app shares the platform's already-loaded design-system instance when ranges are compatible, and falls back to its own bundled copy when they aren't — never blocking its own deploy. The theme is imported only for standalone local development; in production the mounted components inherit the platform shell's theme from the shared document. | true |
| SessionConsumption | The app is a session **consumer only**: it reads `SessionContract` (`currentUser`, `permissions`, `isAuthenticated`) from `@platform/contracts` — the same singleton instance the platform reads — and never implements its own login flow or keeps its own session copy. If `isAuthenticated` is false it renders a "not authenticated" state and defers to the platform. Authorization checks, where the app makes them, are permission strings, never role names. | true |

### Deliberately not rows

- **The exposed module / `remoteEntry`** is the baseline structure the family *is*, described above, not a feature.
- **`SessionContract` itself** is defined by `platform-app`'s `Authentication` feature — this family only *consumes* it.

## Aspirational candidates (owner asked to consider)

| Candidate | Rationale | Shape |
| --- | --- | --- |
| InternalArchitectureAdoption | `solution-platform-embeddability` states the embeddable repo "is free to apply solution #1's structure internally if it also chooses Nx". A future VP: does this embeddable app reuse `platform-app`'s own feature models internally (`NxWorkspaceStructure`, `TieredStateManagement`, `FacadeClientDataAccess`, `SignalForms`, `BusinessLayerTesting`, …)? If yes, the embeddable-app model becomes a thin federation wrapper around a full `platform-app`-shaped internal build. | optional, whole-repo |

`platform-app`'s other aspirational candidates (SSR is impossible for a federated remote; i18n, telemetry, feature-flags, runtime-config) would only apply *via* `InternalArchitectureAdoption`.

## Open questions on V1

1. **`solution-authentication` is shared with `platform-app` and does two very different jobs.** On the platform side it *owns* auth (token, silent refresh, guards, directive, publishes `SessionContract`). On the embeddable side it only *consumes* `SessionContract`. The V1 solution bundles both. **Working hypothesis: `solution-authentication` splits during delta-conflict-detection** — `solution-authentication` (platform, full) and a `solution-session-consumption` (embeddable, read-only). This family's `SessionConsumption` points at the latter.
2. **`solution-platform-embeddability` and `solution-design-system-application` are likewise two-sided** (host config vs remote config). Same working hypothesis: honest split during delta-conflict-detection; this family points at the remote/consumer halves.
3. **Is `SessionConsumption` really mandatory?** An embeddable app that needs no user context at all (a public widget) could skip `@platform/contracts`' `SessionContract` entirely. **Working hypothesis: `FederationRemoteContract` is mandatory; `SessionConsumption` and `DesignSystemFederationConsumption` are "mandatory if the app shows user-scoped data / uses design-system components" — i.e. near-universal but technically optional.** Owner call; if they are optional this family gets its first two real VPs.
4. **`plateau-embeddable-app`'s `parent_plateau` is `plateau-platform-monolith`.** That is odd — the embeddable app is a *separate repo* with a *different baseline*, not a continuation of the platform chain. **Working hypothesis: in v3.1 this family's plateau(s) have empty `parent_plateaus`** (built from scratch), and the relationship to `platform-app` is expressed as a cross-family `Requires` in the umbrella model, not `parent_plateaus`.

## Out of scope

- **This family has no Variation Points today** (pending open question 3). The Variability Map's embeddable-app block will say so and list `InternalArchitectureAdoption` as the single aspirational row.
- **The platform host side** of federation, design-system sharing, and session publication belongs to `platform-app`'s model (`FederationHost`, `DesignSystemConsumption`, `Authentication`).
- **The embeddable app's own internal architecture is deliberately unconstrained** unless `InternalArchitectureAdoption` is later adopted — this is a contract-conformance family, not a build-guide family.
