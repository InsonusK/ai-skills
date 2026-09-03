---
name: plateau-embeddable-app--repo-embeddable-app
description: The baseline any independently deployed remote must satisfy to be loadable by a federation host — a Native Federation remoteEntry exposing one module (a Routes array), singleton @platform/contracts + Angular, hierarchical route ownership one level down, an independent CI/CD pipeline, plus VP1 session consumption and VP2 design-system consumption. Any tooling; imposes no internal architecture. — embeddable-app plateau
domain: skill
type: template
whenToUse: when scaffolding a new embeddable-app repository, its federation.config.mjs / exposed module, wiring SessionContract or design-system consumption, or checking the remote satisfies the host contract
plateau: embeddable-app
version: 20260903180000
tags:
  - skill/template/repo
  - plateau/embeddable-app
  - stack/typescript
  - framework/angular
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]]"
---

> **The `embeddable-app` catalog's single plateau — built from scratch (`parent_plateaus: []`).** A remote is not a continuation of the platform chain: its own internal architecture is unconstrained (a remote *may* adopt the [monolith](skills/angular/architecture/v3.1/monolith/variability-map.md) catalog's feature models — the aspirational `RemoteInternalArchitecture` — but is not required to). This plateau prescribes **only the federation boundary** plus VP1 (`RemoteSessionConsumption`) and VP2 (`RemoteDesignSystemConsumption`). It is a **separate repository**, any tooling — a plain Angular CLI workspace is sufficient.

# Structure

## Workspace Structure

```
/{embeddable-app-name}                (its own repo, its own CI/CD)
  federation.config.mjs               <- withNativeFederation: exposes { './Routes': ... }; @platform/contracts + Angular
                                         singleton strictVersion:true; design-system singleton strictVersion:false + real requiredVersion
  /src
    main.ts                           <- initFederation({}, ...) BEFORE import('./bootstrap')
    styles.scss                       <- @use design-system theme.scss/custom-tokens.scss — standalone dev only (redundant once mounted)
    /app
      [remote.routes.ts](./classes/plateau-embeddable-app--class-remote-routes.skill.md)   <- the exposed module: REMOTE_ROUTES, root-relative only
      /session
        [require-permission.ts](./classes/plateau-embeddable-app--class-require-permission.skill.md)   <- CanActivateFn reading SESSION_CONTRACT
        [has-permission.directive.ts](./classes/plateau-embeddable-app--class-has-permission-directive.skill.md)   <- *hasPermission
      /{feature}
        {feature}.component.ts         <- renders a not-authenticated state when isAuthenticated() is false
      app.ts / app.routes.ts           <- standalone dev shell: mounts REMOTE_ROUTES at its own root
  package.json                         <- @angular-architects/native-federation; @platform/contracts (compatible range); design-system (requiredVersion)
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| federation.config.mjs | `withNativeFederation({ name, exposes: { './Routes': './src/app/remote.routes.ts' }, shared: { ...shareAll(strict), '@platform/contracts': strict, 'design-system': {strictVersion:false} } })`. | — |
| src/app/remote.routes.ts | The federation-exposed module — `REMOTE_ROUTES: Routes`. Mounts this remote's own features' root segments; never references the segment the host mounts it at. | [[skills/angular/architecture/v3.1/embeddable-app/plateau/plateau-embeddable-app/structure/classes/plateau-embeddable-app--class-remote-routes.skill\|class-remote-routes]] |
| src/app/session/require-permission.ts | `requirePermission(perm): CanActivateFn` — reads `SESSION_CONTRACT`; `isAuthenticated() === false` → `false` (the host owns the redirect). | [[skills/angular/architecture/v3.1/embeddable-app/plateau/plateau-embeddable-app/structure/classes/plateau-embeddable-app--class-require-permission.skill\|class-require-permission]] |
| src/app/session/has-permission.directive.ts | `*hasPermission="'x'"` — shows/hides by a permission string from `SESSION_CONTRACT`. Same model as the host's directive. | [[skills/angular/architecture/v3.1/embeddable-app/plateau/plateau-embeddable-app/structure/classes/plateau-embeddable-app--class-has-permission-directive.skill\|class-has-permission-directive]] |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|routes.ts.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/Implementation/federation.extend.md|federation.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major | Native Federation remote build plugin — builds `federation.config.mjs` into a `remoteEntry.json` |
| @platform/contracts | semver range compatible with the platform's declared range | The one host↔remote contract; `singleton: true, strictVersion: true` |
| design-system | version-negotiated `requiredVersion`, this team's real tested range | Theme + `ds-*` components; `singleton: true, strictVersion: false` — a mismatch falls back to a bundled copy, never blocks this remote's deploy |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/Implementation/federation.extend.md|federation.extend]]

# Rules

## MUST
- The repo exposes a valid `remoteEntry` and **one** exposed module (`./Routes` — a `Routes` array).
- Angular and `@platform/contracts` are `singleton: true, strictVersion: true` — host and remote run one runtime, one contract instance.
- The exposed module's routes are **root-relative only** — no mount prefix baked in; the host assigns the segment.
- Never import `platform-shell` internals; never export anything the host imports beyond the exposed module. The only contract is `@platform/contracts` + the federation boundary.
- **VP1** — session/permission state is read **exclusively** through `SESSION_CONTRACT`: no login flow, no local session copy. `isAuthenticated() === false` → render a not-authenticated state, never trigger a navigation to a login route. Every authorization check is a permission string, matching the host's `*hasPermission` semantics.
- **VP2** — `design-system` is declared `singleton: true, strictVersion: false` with this team's **accurate** `requiredVersion` (never a wildcard); `src/styles.scss` still imports the theme for correct standalone dev.
- The repo builds, tests, and deploys on its **own** pipeline; a `@platform/contracts` major bump is coordinated cross-team.

## SHOULD
- Periodically update `design-system`'s `requiredVersion` toward the platform's targeted version, to get the shared-instance benefit rather than routinely running an isolated copy.
- Keep a minimal "sign in to continue" placeholder — do not duplicate the host's full forbidden/login UI.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/Implementation/federation.extend.md|federation.extend]]

# Check list

- [ ] The remote exposes a valid `remoteEntry` and one exposed module (`./Routes`)
- [ ] Angular + `@platform/contracts` are `singleton: true, strictVersion: true`
- [ ] The exposed routes are root-relative, no mount prefix baked in
- [ ] No import between the remote's internals and `platform-shell`'s internals
- [ ] `SESSION_CONTRACT` is the only session source; no login route, no local session state; `isAuthenticated: false` → not-authenticated state, no redirect
- [ ] `design-system` is `strictVersion: false` with an accurate `requiredVersion`; `styles.scss` imports the theme
- [ ] The remote builds/tests/deploys on its own pipeline

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]

# Unittest TestCases

- [ ] WHEN the remote is loaded standalone (local dev) THEN it still renders — no dependency on `platform-shell` internals — and applies the theme via its own `styles.scss`
- [ ] WHEN the host loads this remote's `remoteEntry` THEN the exposed `./Routes` mounts and its components read `SESSION_CONTRACT`
- [ ] WHEN this remote reads `SESSION_CONTRACT.permissions()` THEN it matches the set the host's own `*hasPermission` uses
- [ ] WHEN the host session expires THEN `SESSION_CONTRACT.isAuthenticated()` becomes `false` here with no action on this remote's part
- [ ] WHEN this remote's `design-system` `requiredVersion` matches the host's loaded version THEN no separate design-system bundle is fetched

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|session-consumption.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/solution-remote-design-system-consumption.skill.md|solution-remote-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-remote-design-system-consumption.skill/Implementation/federation.extend.md|federation.extend]]
