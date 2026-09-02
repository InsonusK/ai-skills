---
tags:
  - concern/architecture
  - stack/typescript
---

# embeddable-app Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/v3.1/embeddable-app/feature/feature-model.md|embeddable-app/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/v3.1/monolith/variability-map.md|monolith]], [[skills/angular/architecture/v3.1/platform-host/variability-map.md|platform-host]], [[skills/angular/architecture/v3.1/design-system/variability-map.md|design-system]].

**Status.** `v3.1/solutions/` holds the migrated + split + new solutions (Stage 3). **Realized by** links point into it.

A remote is built and deployed by its own team, in its own repository, **without knowing which host will load it**. Constraints that reference the host (`platform-host:VPn`) are *"meaningful only if"* notes, not legality gates — a remote configured for session consumption simply reads `isAuthenticated: false` if the host has no `SessionSharing`.

## Variation Points

Common baseline (`FederationRemoteContract`) is not a row — every loadable remote satisfies it. See [Features that are not VPs](#features-that-are-not-vps).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **RemoteSessionConsumption** — does the remote read `SessionContract` (`currentUser` / `permissions` / `isAuthenticated`) from `@platform/contracts` and gate its own UI on it? | Yes / No | — (no legality gate; *meaningful only if* the host has [`platform-host:VP2` SessionSharing](skills/angular/architecture/v3.1/platform-host/variability-map.md)) | Yes → new `solution-session-consumption` (Stage 3, the remote-read half of V1 [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md\|solution-authentication]]) | Cross-catalog: reads the `SessionContract` shape from `@platform/contracts` (`platform-host:PlatformContracts`) | No |
| VP2 | **RemoteDesignSystemConsumption** — does the remote declare `design-system` as a version-negotiated federation singleton with an accurate `requiredVersion`? | Yes / No | — | Yes → new `solution-remote-design-system-consumption` (Stage 3, the remote half of V1 [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md\|solution-host-design-system-consumption]]) | Cross-catalog: version-negotiates with the host's [`platform-host:VP1` HostDesignSystemConsumption](skills/angular/architecture/v3.1/platform-host/variability-map.md); consumes the [[skills/angular/architecture/v3.1/design-system/variability-map.md\|design-system]] package | No |
| VP3 | **RemoteInternalArchitecture** *(aspirational — no solution yet)* — does the remote adopt the [[skills/angular/architecture/v3.1/monolith/feature/feature-model.md\|monolith]] catalog's internal feature models inside its own repo (so it is a full monolith wrapped in a federation entry point)? | Yes / No | — | *(none)* — when built, a `RemoteInternalArchitecture=Yes` plateau `parent_plateaus` a `monolith/` plateau | Composition: pulls in the entire `monolith/` VP space as the remote's internal build | No |

### VP1 / VP2 — why "near-universal but optional"

V1's single `plateau-embeddable-app` composes both `solution-authentication` (session consumption) and `solution-design-system-application` (design-system consumption), which reads as "both mandatory". Feature-model open question 2 argues otherwise: a public widget with no user context can skip `SessionContract`; a remote that renders no shared-styled surface can skip the design system. **Owner ruling (this session): both are variable** — this catalog's first two real VPs. `FederationRemoteContract` remains the only common feature.

## Features that are not VPs

- **`FederationRemoteContract`** — every loadable remote satisfies it (valid `remoteEntry`, exposed module, `singleton` Angular + `@platform/contracts`, hierarchical route ownership, no internal cross-imports, independent CI/CD). Shared core, not variability. Realized by the remote half of the split V1 `solution-platform-embeddability` → v3.1 `solution-federation-remote`.
- **`SessionContract` the type** — owned and published by `platform-host`'s `PlatformContracts`; VP1 only reads it.
- **The remote's own internal architecture**, if any — that is `monolith/`'s VP space, reached through VP3, not re-modeled here.

## Plateau Map derivation

**No plateaus exist in `v3.1/embeddable-app/` yet.**

### Reference: V1 → v3.1

| V1 plateau | v3.1 embeddable-app plateau | VP answers |
| --- | --- | --- |
| `plateau-embeddable-app` (parent `plateau-platform-monolith` — **wrong**, feature-model open question 1) | e.g. `plateau-embeddable-app` (`parent_plateaus` empty) | VP1=Yes, VP2=Yes; VP3=No |

### Combinations v3.1 allows that V1 has no plateau for

- **VP1=No** — a remote with no user-scoped data (a public widget).
- **VP2=No** — a remote that renders no design-system-styled UI (a canvas/chart embed, an iframe-like tool).
- **VP1=No, VP2=No** — the minimal remote: just `FederationRemoteContract`.
- **VP3=Yes** — a remote that is internally a full monolith (aspirational).

## Out of scope

- **Realized-by links are provisional** (Stage 3 creates the three new remote-side solutions).
- **The host side** is [[skills/angular/architecture/v3.1/platform-host/variability-map.md|the platform-host map]].
- **`Migration = No`** — a remote's session/design-system consumption is set at build time by its own team; no observed post-deployment transition.
