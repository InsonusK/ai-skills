---
name: plateau-platform-host--repo-platform-host
description: The multiuser-monolith Nx workspace turned into a Native Federation dynamic host — apps/platform-shell gains type:host, federation shared-dependency rules (@platform/contracts + Angular strict singletons), and a runtime remote manifest. Every monolith project is inherited from plateau-multiuser-monolith. — platform-host plateau
domain: skill
type: template
whenToUse: when turning the shell workspace into a federation host, adding the type:host tag or a federation shared-dependency rule, or checking a host/remote boundary — for any monolith-side project rule, read plateau-multiuser-monolith's structure
plateau: platform-host
version: 20260903180000
tags:
  - skill/template/repo
  - plateau/platform-host
  - stack/typescript
  - framework/angular
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]]"
---

> **The `platform-host` catalog's single plateau composes [`plateau-multiuser-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/plateau-multiuser-monolith.skill/plateau-multiuser-monolith.skill.md) cross-catalog via `parent_plateaus`.** Every monolith project — `libs/shared/*`, `libs/{feature}/*`, `apps/platform-shell`, its e2e, the component-preview — is carried forward from that plateau's [`structure/`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/plateau-multiuser-monolith--repo-multiuser-monolith.skill.md) unchanged. This plateau's `structure/` records **only the federation delta**, matching how the [platform-host variability map](skills/angular/architecture/v3.1/platform-host/variability-map.md) is scoped.

# What the federation delta adds

- **`apps/platform-shell` gains `type:host`** (in addition to `type:app`, `scope:platform`) and becomes a **Native Federation dynamic host** — `federation.config.mjs`, `src/main.ts` calls `initFederation(...)` before bootstrap, and a `RemoteRegistryService` resolves remotes from a runtime manifest. See [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/plateau-platform-host--project-platform-shell.skill.md|project-platform-shell]].
- **`@platform/contracts`** — its **own repository / published npm package** (types + DI tokens only, no implementation), declared `singleton: true, strictVersion: true` on the host. See [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-contracts/plateau-platform-host--repo-platform-contracts.skill.md|repo-platform-contracts]].
- **`HostSession`** — the host is the **only** provider of `SESSION_CONTRACT` (VP2 `SessionSharing`); a read-only signal view over `libs/shared/state`'s `auth` slice. See [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/classes/plateau-platform-host--class-host-session.skill.md|class-host-session]].
- **`federation.config.mjs`** also declares `design-system` `singleton: true, strictVersion: false` (VP1 `HostDesignSystemConsumption` — version-negotiated, never lockstep), and `apps/platform-shell/src/styles` imports the design-system `theme.scss` / `custom-tokens.scss` — the only production consumer required to.
- **A 5th service-worker rule** (VP3 `FederatedReadResilience`) — stale-while-revalidate for federated remote chunks, `KNOWN_REMOTE_ORIGINS` derived from the same manifest `RemoteRegistryService` uses. Applies **only** because the parent monolith has `OfflineReadResilience` (VP4). See [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/classes/plateau-platform-host--class-service-worker.skill.md|class-service-worker]].

## NPM Packages (added)

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major (22.1.x here) | Native Federation host runtime + `@angular/build` build plugin + `loadRemoteModule` |
| @platform/contracts | semver range, published from its own repo | The sole build-time host↔remote contract; a strict shared singleton |

## Directory and project skills (delta only)

| Directory | template link | Description |
| ---------- | ------------- | ----------- |
| /apps/platform-shell | [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/plateau-platform-host--project-platform-shell.skill\|project-platform-shell]] | The federation delta on the monolith's own [`project-platform-shell`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/platform-shell/plateau-multiuser-monolith--project-platform-shell.skill.md). |
| `@platform/contracts` (own repo) | [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-contracts/plateau-platform-host--repo-platform-contracts.skill\|repo-platform-contracts]] | The published contract package. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/Implementation/platform-shell.federation.extend.md|platform-shell.federation.extend]]

# Rules

## MUST
- `apps/platform-shell` declares `type:host` in addition to `type:app` / `scope:platform`.
- `federation.config.mjs` declares `@platform/contracts` (and Angular) `singleton: true, strictVersion: true` — an incompatible major on a remote is a visible load-time failure, never a silently duplicated runtime.
- `federation.config.mjs` declares `design-system` `singleton: true, strictVersion: false` — never `strictVersion: true` (that reintroduces lockstep upgrades).
- The list of remotes and their `remoteEntry` URLs is resolved at **runtime** (a Dynamic Federation manifest), never hardcoded into the host's build.
- Never bundle a specific remote's code at build time; never import a remote's internals into the host, or the host's into a remote — the only contract is `@platform/contracts` + the federation `remoteEntry` boundary.
- `@platform/contracts` is its own repository, published to npm, exporting types / interfaces / DI tokens **only** — no runtime implementation, no Angular compilation.
- The 5th SW rule is registered **after** the base network-only (auth/mutations) rule, and `KNOWN_REMOTE_ORIGINS` is derived from the same manifest `RemoteRegistryService` uses.
- Every monolith-side rule from [`plateau-multiuser-monolith`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/plateau-multiuser-monolith--repo-multiuser-monolith.skill.md) still applies unchanged.

## SHOULD
- Never cache the remotes manifest for the whole tab lifetime with no refresh path — a newly onboarded remote should become visible without a full reload.
- Never hardcode `KNOWN_REMOTE_ORIGINS` as a separate static list — derive it from the manifest.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/Implementation/platform-shell.federation.extend.md|platform-shell.federation.extend]]

# Check list

- [ ] `apps/platform-shell` is tagged `type:host` and resolves remotes at runtime, not build time
- [ ] `@platform/contracts` + Angular are `singleton: true, strictVersion: true`; `design-system` is `strictVersion: false`
- [ ] A new remote is onboarded by updating the manifest alone — no host rebuild
- [ ] A failed remote load degrades to a fallback slot, not a shell-wide crash
- [ ] No import between host internals and any remote's internals beyond `@platform/contracts` + the federation boundary
- [ ] `@platform/contracts` exports only types / interfaces / DI tokens
- [ ] The 5th SW rule (federated chunks) is present only because the parent monolith has offline-first, and registered after the network-only rule

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/Repository.extend.md|Repository.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/ServiceWorker/service-worker.ts.extend.md|ServiceWorker/service-worker.ts.extend]]
