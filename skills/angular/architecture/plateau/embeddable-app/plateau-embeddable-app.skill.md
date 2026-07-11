---
name: plateau-embeddable-app
description: The baseline structure any independently deployed, separately repositoried embeddable application must follow to be loadable by the platform host — Native Federation remote config, version-negotiated design-system and @platform/contracts singletons, and read-only session consumption via SessionContract
domain: skill
type: template
version: 20260711140000
tags:
  - skill/template/plateau
  - plateau/embeddable-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill.md|solution-platform-embeddability]]"
  - "[[skills/angular/architecture/solutions/solution-design-system-application.skill/solution-design-system-application.skill.md|solution-design-system-application]]"
  - "[[skills/angular/architecture/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
parent_plateau:
---

> This is a sibling plateau to [[skills/angular/architecture/plateau/platform/plateau-platform.skill.md|platform]], not a continuation of the main application's own plateau chain. It lives in its own repository — separate from both the main Nx platform monorepo and the design-system repository — owned and deployed independently by its own team. Any independently deployed application in this architecture, regardless of which team builds it, must conform to this plateau's structure to be embeddable. It becomes loadable by the platform host once the [[skills/angular/architecture/plateau/platform/plateau-platform.skill.md|platform]] plateau exists (`apps/platform-shell` turned into a Native Federation dynamic host).

# Core Principles

- An embeddable app repository is not required to adopt Nx or any platform-specific tooling — it only needs to satisfy the federation contract: `remoteEntry`, exposed module, and the shared singleton dependencies described below
- The only contract with the platform is `@platform/contracts` plus the federation `remoteEntry`/exposed-module boundary — no import of platform-shell internals in either direction
- Angular and `@platform/contracts` are shared as strict `singleton: true` dependencies; the design system is shared as a version-negotiated singleton (`singleton: true`, `strictVersion: false`) — sharing happens automatically when each side's declared version range is compatible, falling back to an isolated copy otherwise
- The app is a session consumer only: it reads `SessionContract` (`currentUser`, `permissions`, `isAuthenticated`) from `@platform/contracts` and never implements its own login flow or maintains its own copy of session state
- The app builds, tests, and deploys entirely on its own CI/CD pipeline, independent of the platform's own release schedule

# Capabilities

- federation
  - Exposes one or more entry points (typically a top-level component) via a Native Federation remote config, discoverable by the platform's runtime remote registry — no platform code change needed to onboard
- design-system consumption
  - Declares its own accurate `requiredVersion` range for the design system; automatically shares the platform's instance when ranges are compatible, isolates when they aren't, with no manual coordination beyond keeping that range current
  - Imports the design system's theme in its own root styles for correct standalone local development, understanding this is redundant — not incorrect — once mounted inside the platform in production
- session
  - Reads the platform's authenticated session and permissions through `SessionContract`, with no login screen or session state of its own
  - Renders its own "not authenticated" state when loaded without a session, rather than attempting authentication itself
- independence
  - Own CI pipeline, own deploy target, own choice of internal tooling (Nx optional) — ships on its own schedule with no coordination required for routine releases

# Usecases

## Onboard a new embeddable app repository

```mermaid
sequenceDiagram
    autonumber
    actor Team as Embeddable-app team
    participant Repo as {embeddable-app-name} repo
    participant Manifest as Platform runtime manifest
    participant Host as platform-shell (host)

    Team->>Repo: scaffold federation.config.ts (exposes entry component)
    Team->>Repo: declare Angular, @platform/contracts, design system as shared deps
    Team->>Repo: build & deploy independently, own CI pipeline
    Team->>Manifest: publish remoteEntry URL + exposed module path
    Host->>Manifest: refresh remotes manifest
    Host->>Repo: loadRemoteModule(remoteEntry, exposedModule)
    Repo-->>Host: exposed component mounts
    Host->>Repo: shared singletons resolve (Angular, @platform/contracts, design system if compatible)
    Repo->>Repo: reads SessionContract.currentUser/permissions/isAuthenticated
```

## Run the embeddable app standalone during local development

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant App as {embeddable-app-name} (standalone)
    participant Theme as own styles.scss

    Dev->>App: ng serve (outside the platform)
    App->>Theme: import design-system theme.scss/custom-tokens.scss
    Theme-->>App: correct visual rendering, no platform host present
    Note over App: SessionContract is unavailable standalone —<br/>app renders its own "not authenticated" state
```
