---
tags:
  - concern/architecture
  - stack/typescript
---

# embeddable-app Variability Map

Built per [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]], from the non-common features of [[skills/angular/architecture/embeddable-app/feature/feature-model.md|embeddable-app/feature/feature-model.md]]. Sibling catalogs: [[skills/angular/architecture/monolith/variability-map.md|monolith]], [[skills/angular/architecture/platform-host/variability-map.md|platform-host]], [[skills/angular/architecture/design-system/variability-map.md|design-system]].

**Status.** `v3.1/solutions/` holds the migrated / split / new solutions; every **Realized by** cell links one of them. The plateau↔VP view lives in [[skills/angular/architecture/embeddable-app/plateau/plateau-repository.md|embeddable-app/plateau/plateau-repository.md]] ([[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md|plateau-map-create]]).

A remote is built and deployed by its own team, in its own repository, **without knowing which host will load it**. Constraints that reference the host (`platform-host:VPn`) are *"meaningful only if"* notes, not legality gates — a remote configured for session consumption simply reads `isAuthenticated: false` if the host has no `SessionSharing`.

## Variation Points

Common baseline (`FederationRemoteContract`) is not a row — every loadable remote satisfies it. See [Features that are not VPs](#features-that-are-not-vps).

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | **RemoteSessionConsumption** — does the remote read `SessionContract` (`currentUser` / `permissions` / `isAuthenticated`) from `@platform/contracts` and gate its own UI on it? | Yes / No | — (no legality gate; *meaningful only if* the host has [`platform-host:VP2` SessionSharing](skills/angular/architecture/platform-host/variability-map.md)) | Yes → [[skills/angular/architecture/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md\|solution-session-consumption]] (the remote-read half, carved from V1 `solution-authentication`) | Cross-catalog: reads the `SessionContract` shape from `@platform/contracts` (`platform-host:PlatformContracts`) | No |
| VP2 | **RemoteDesignSystemConsumption** — does the remote declare `design-system` as a version-negotiated federation singleton with an accurate `requiredVersion`? | Yes / No | — | Yes → [[skills/angular/architecture/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md\|solution-remote-design-system-consumption]] (the remote half; split from V1 `solution-design-system-application`) | Cross-catalog: version-negotiates with the host's [`platform-host:VP1` HostDesignSystemConsumption](skills/angular/architecture/platform-host/variability-map.md); consumes the [[skills/angular/architecture/design-system/variability-map.md\|design-system]] package | No |
| VP3 | **RemoteInternalArchitecture** *(aspirational — no solution yet)* — does the remote adopt the [[skills/angular/architecture/monolith/feature/feature-model.md\|monolith]] catalog's internal feature models inside its own repo (so it is a full monolith wrapped in a federation entry point)? | Yes / No | — | *(none — aspirational)* — when built, a `RemoteInternalArchitecture=Yes` plateau `parent_plateaus` a `monolith/` plateau | Composition: pulls in the entire `monolith/` VP space as the remote's internal build | No |

### VP1 / VP2 — why "near-universal but optional"

V1's single `plateau-embeddable-app` composes both `solution-authentication` (session consumption) and `solution-design-system-application` (design-system consumption), which reads as "both mandatory". Feature-model open question 2 argues otherwise: a public widget with no user context can skip `SessionContract`; a remote that renders no shared-styled surface can skip the design system. **Owner ruling (this session): both are variable** — this catalog's first two real VPs. `FederationRemoteContract` remains the only common feature.

## Features that are not VPs

- **`FederationRemoteContract`** — every loadable remote satisfies it (valid `remoteEntry`, exposed module, `singleton` Angular + `@platform/contracts`, hierarchical route ownership, no internal cross-imports, independent CI/CD). Shared core, not variability. Realized by the remote half of the split V1 `solution-platform-embeddability` → v3.1 `solution-federation-remote`.
- **`SessionContract` the type** — owned and published by `platform-host`'s `PlatformContracts`; VP1 only reads it.
- **The remote's own internal architecture**, if any — that is `monolith/`'s VP space, reached through VP3, not re-modeled here.

## Out of scope

- **The plateau↔VP view** — the `plateau-embeddable-app` matrix and the V1 reference mapping — lives in [[skills/angular/architecture/embeddable-app/plateau/plateau-repository.md|plateau/plateau-repository.md]], not here ([[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map.md|ADR]]).
- **The host side** is [[skills/angular/architecture/platform-host/variability-map.md|the platform-host map]].
- **`Migration = No`** — a remote's session/design-system consumption is set at build time by its own team; no observed post-deployment transition.
