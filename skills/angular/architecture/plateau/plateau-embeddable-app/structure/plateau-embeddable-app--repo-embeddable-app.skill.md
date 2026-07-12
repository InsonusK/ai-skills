---
name: plateau-embeddable-app--repo-embeddable-app
description: Baseline repository structure any independently deployed embeddable application must follow to be loadable by the platform host — Native Federation remote config, @platform/contracts and the design system as shared dependencies, SessionContract as the sole session source — embeddable-app plateau
domain: skill
type: template
plateau: embeddable-app
version: 20260711140000
tags:
  - skill/template/repo
  - plateau/embeddable-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
---

> This is a separate repository from the platform monorepo, owned and deployed independently by its own team. It is not required to be an Nx workspace — a plain Angular CLI workspace is sufficient, since this repo hosts a single deployable unit and does not need affected-based builds or cross-project boundary enforcement the way the platform monorepo does. The repository does not split into distinguishable sub-projects (unlike, e.g., the [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|design-system]] repo's library/demo split) — it is one flat app, so this plateau has no dedicated project-tier skill; everything below the repository level is documented here directly.

# Structure

## Workspace Structure

```
/{embeddable-app-name}
  /src
    federation.config.ts     <- Native Federation remote config: exposes entry point(s), declares Angular, @platform/contracts, and the design system as shared dependencies
    /app
      app.component.ts       <- top-level exposed component; reads SessionContract from @platform/contracts
    styles.scss               <- imports the design system's theme.scss/custom-tokens.scss, for standalone local development only
  package.json                <- declares @angular-architects/native-federation, @platform/contracts, and the design system as dependencies
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/EmbeddableApp/federation.extend|EmbeddableApp/federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

## Directory and project skills

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /src | — | The embeddable app itself — the repository's single deployable unit, no Nx apps/libs split required or expected. Federation remote config, the exposed root component, and root styles all live here. No project-tier skill exists for this plateau; see Workspace Structure above for file-level detail. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major version in use | Federation remote build plugin; builds `federation.config.ts` into a `remoteEntry` |
| @platform/contracts | semver range compatible with the platform's declared range | Shared EventBus/`SessionContract` singleton; MUST be the same singleton contract the platform host shares |
| design system (per [[skills/angular/architecture/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]'s npm package) | version-negotiated `requiredVersion` range, kept current as this team adopts newer releases | Theme (`theme.scss`/`custom-tokens.scss`) and `ds-*` components; shared with the platform's instance when ranges are compatible, isolated copy otherwise |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/EmbeddableApp/federation.extend|EmbeddableApp/federation.extend]]

# Rules

## MUST
- The repository MUST publish its `remoteEntry` and exposed module path to a location the platform's runtime remote registry can discover — how that publication happens (a manifest file, a registration API call, etc.) is an operational detail owned by this repository's own deploy pipeline.
- The repository MUST declare `@platform/contracts` as a `singleton: true` shared dependency in its federation config, at a version compatible with the platform's expected range.
- `federation.config.ts` MUST declare `requiredVersion` for the design system as an accurate reflection of the version range this app has actually been built and tested against — not left as a wildcard/unbounded range, which would defeat the purpose of version negotiation.
- The team MUST keep the design system's `requiredVersion` updated as they adopt newer design-system versions, so version negotiation continues to reflect real compatibility.
- This app's own `styles.scss` MUST still import the design-system theme, for correct standalone local development — omitting it would make local preview visually incorrect, even though it's redundant once mounted inside the platform in production.
- `SessionContract` MUST be treated as read-only from this app's point of view — the app MUST NOT mutate the session (log in/out, change permissions) through the contract; only the platform's own auth slice does that.
- The app MUST read session/permission state exclusively through `SessionContract` — it MUST NOT implement its own login flow or maintain its own copy of session state.
- If the app is loaded without an authenticated session (`SessionContract.isAuthenticated` is `false`), it MUST render its own "not authenticated" state rather than attempting its own authentication — redirecting to authenticate is the platform's responsibility, not the embeddable app's.

## SHOULD
- The repository SHOULD run its own CI pipeline (lint/test/build/deploy) independent of the platform's pipeline — this is the entire point of the independent-deploy requirement behind this plateau.
- The team SHOULD periodically update the design system's `requiredVersion` to track the platform's currently targeted version, to get the shared-instance benefit (smaller payload, guaranteed consistency) rather than routinely running an isolated copy.

## MUST NOT
- The repository MUST NOT import platform-shell internals directly — the only contract with the platform is `@platform/contracts` plus the federation `remoteEntry`/exposed-module boundary.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/EmbeddableApp/federation.extend|EmbeddableApp/federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Anti-patterns

- **Bumping `@platform/contracts` to an incompatible major version without coordinating with the platform team**
  - Consequence: the federation runtime will either duplicate the shared singleton or fail to load the remote entirely, depending on `strictVersion` configuration
  - Instead: treat `@platform/contracts`' version range as a cross-team contract; coordinate major-version bumps the same way any published API's breaking change would be coordinated
- **Depending on platform-shell's own package instead of only `@platform/contracts`**
  - Consequence: recreates tight coupling to the platform's internals that federation was chosen specifically to avoid
  - Instead: everything the embeddable app needs from the platform must be expressed in `@platform/contracts`
- **Declaring an unbounded or overly permissive `requiredVersion` for the design system (e.g. accepting any major version)**
  - Consequence: the app may end up sharing a design-system version it was never actually tested against, risking subtle visual or behavioral breakage that version negotiation was meant to prevent
  - Instead: declare the actual range this app has been built and verified against, updating it deliberately as that range changes
- **Never updating the design system's `requiredVersion` after the initial setup**
  - Consequence: the app permanently runs its own isolated copy of the design system, missing the shared-instance benefit indefinitely even as the platform and design system both move forward
  - Instead: periodically revisit and update the declared range as part of normal maintenance
- **Implementing its own login screen "just in case" the platform session is missing**
  - Consequence: duplicates authentication logic across teams, and creates two different ways a user could end up authenticated, defeating the single-session model this plateau establishes
  - Instead: only ever read `SessionContract`; the platform alone is responsible for establishing a session

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/EmbeddableApp/federation.extend|EmbeddableApp/federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]

# Unittest TestCases

- [ ] WHEN the embeddable app is loaded standalone (outside the platform, e.g. for local development) THEN
  - [ ] it still renders correctly, since it does not depend on platform-shell internals
  - [ ] it renders with the correct theme, since its own `styles.scss` applies it
- [ ] WHEN the platform host loads this app's `remoteEntry` THEN
  - [ ] the exposed component mounts and can send/receive events through `@platform/contracts`' EventBus
- [ ] WHEN this app is mounted inside the platform and its `requiredVersion` for the design system matches the platform's loaded version THEN
  - [ ] no separate design-system bundle is fetched — the shared instance is used
- [ ] WHEN this app reads `SessionContract.permissions` THEN
  - [ ] it reflects the same permission set the platform's own UI uses for its `*hasPermission` checks
- [ ] WHEN the platform's session expires THEN
  - [ ] `SessionContract.isAuthenticated` becomes `false` for this app, without any action needed on its own part

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/EmbeddableApp/Repository.create|EmbeddableApp/Repository.create]]
- [[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]] - [[skills/angular/architecture/solutions/solution-design-system-application.skill/Implementation/EmbeddableApp/federation.extend|EmbeddableApp/federation.extend]]
- [[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]] - [[skills/angular/architecture/solutions/solution-authentication.skill/Implementation/EmbeddableApp/platform-contracts.extend|EmbeddableApp/platform-contracts.extend]]
