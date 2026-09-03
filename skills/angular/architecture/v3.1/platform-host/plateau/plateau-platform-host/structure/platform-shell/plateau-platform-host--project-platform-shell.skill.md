---
name: plateau-platform-host--project-platform-shell
description: The federation delta on the monolith's platform-shell — Native Federation dynamic host config, initFederation before bootstrap, RemoteRegistryService, remote mounting at one root segment, the SESSION_CONTRACT provider, design-system as a version-negotiated singleton, and the 5th SW rule — platform-host plateau
domain: skill
type: template
whenToUse: when adding federation.config.mjs / initFederation to the shell, wiring RemoteRegistryService or the remote route mount, providing SESSION_CONTRACT, or checking the shell's federation rules — for the monolith-side shell rules read plateau-multiuser-monolith's project-platform-shell
plateau: platform-host
project_kind: application
version: 20260903180000
tags:
  - skill/template/project
  - plateau/platform-host
  - stack/typescript
  - framework/angular
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]]"
---

> The federation delta ONLY. Everything else about `apps/platform-shell` — routing, root providers, the `SelectivePreloadingStrategy`, bundle budgets, the Workbox SW, `GlobalErrorHandler`, `authInterceptor`, the bootstrap silent refresh — is [`plateau-multiuser-monolith`'s `project-platform-shell`](skills/angular/architecture/v3.1/monolith/plateau/plateau-multiuser-monolith/structure/platform-shell/plateau-multiuser-monolith--project-platform-shell.skill.md), carried forward unchanged.

# Goal

- Load independently built, independently deployed remotes into `apps/platform-shell` at runtime, sharing one Angular runtime and one `@platform/contracts` instance — with no host rebuild when a remote ships

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md|platform-shell.project.extend]]

# Structure

## Project Structure (added)

```
/apps/platform-shell
  federation.config.mjs                 <- Native Federation: shareAll strict singletons + explicit
                                           @platform/contracts (strict) and design-system (strictVersion:false)
  /src
    main.ts                             <- initFederation('federation.manifest.json', ...) BEFORE import('./bootstrap')
    bootstrap.ts                        <- the real bootstrapApplication(App, appConfig)
    /app
      /remote-registry
        [remote-registry.service.ts](./classes/plateau-platform-host--class-remote-registry-service.skill.md)   <- runtime manifest → loadRemoteModule
      /session
        [host-session.ts](./classes/plateau-platform-host--class-host-session.skill.md)   <- the ONLY SESSION_CONTRACT provider (VP2)
      app.config.ts                     <- + { provide: SESSION_CONTRACT, useExisting: HostSession }
                                           + provideAppInitializer(() => inject(RemoteRegistryService).load())
      app.routes.ts                     <- a remote mounted at one root segment via loadChildren → REMOTE_ROUTES
      remote-unavailable.component.ts    <- the fallback slot for a failed remote load
    sw-src.ts                           <- + 5th rule: stale-while-revalidate for KNOWN_REMOTE_ORIGINS (VP3)
    styles.scss / src/styles            <- @use design-system theme.scss + custom-tokens.scss at the root
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| federation.config.mjs | `withNativeFederation({ name: 'platform-host', shared: { ...shareAll({singleton,strictVersion}), '@platform/contracts': strict, 'design-system': {strictVersion:false} } })`. No remotes declared at build time. | — |
| src/main.ts | `initFederation('federation.manifest.json', { hostRemoteEntry: {...} }).then(() => import('./bootstrap'))`. | — |
| /remote-registry/remote-registry.service.ts | Fetches the remotes manifest at runtime; `loadRemote(name)` → `loadRemoteModule({ remoteEntry, exposedModule })`; a missing remote **rejects** (caller renders a fallback), never throws at bootstrap. | [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/classes/plateau-platform-host--class-remote-registry-service.skill\|class-remote-registry-service]] |
| /session/host-session.ts | Implements `SessionContract` as a read-only signal view over `libs/shared/state`'s `auth` slice; provided once, here, under `SESSION_CONTRACT`. | [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/classes/plateau-platform-host--class-host-session.skill\|class-host-session]] |
| app.routes.ts | One `loadChildren` entry per remote root segment: `async () => { try { const m = await registry.loadRemote('x'); return m.REMOTE_ROUTES; } catch { return [{ path:'', component: RemoteUnavailableComponent }]; } }`. | — |
| sw-src.ts | 5th `registerRoute` — `StaleWhileRevalidate` for `KNOWN_REMOTE_ORIGINS` (from the manifest), registered after the base network-only rule. | [[skills/angular/architecture/v3.1/platform-host/plateau/plateau-platform-host/structure/platform-shell/classes/plateau-platform-host--class-service-worker.skill\|class-service-worker]] |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md|platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/Implementation/platform-shell.federation.extend.md|platform-shell.federation.extend]]

# Rules

## MUST
- `federation.config.mjs` never lists a remote's `remoteEntry` statically — `RemoteRegistryService` resolves them at runtime.
- Anything in the shell that talks to a mounted remote does so **only** through `@platform/contracts` — never a remote's internal exports.
- `SESSION_CONTRACT` is provided **exactly once**, here, bound to the host's `auth`-slice-backed `HostSession`. The contract is read-only — no remote can log in/out or change permissions through it.
- The design-system shared dependency is `strictVersion: false`; the shell's root styles import the design-system `theme.scss` + `custom-tokens.scss`.
- `main.ts` runs `initFederation(...)` before `import('./bootstrap')` — bootstrap never happens before the federation runtime is initialised.
- A failed `loadRemote` is caught at the mount point and renders a fallback in that route slot — it never propagates to a shell-wide error.

## SHOULD
- Treat a manifest fetch failure at bootstrap as recoverable (the app still loads; remote-backed routes show a fallback), not fatal.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md|platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Check list

- [ ] `federation.config.mjs` marks `@platform/contracts` + Angular `singleton: true, strictVersion: true`, `design-system` `strictVersion: false`
- [ ] No remote's `remoteEntry` is hardcoded in the shell's source or build config
- [ ] `SESSION_CONTRACT` has exactly one provider (`HostSession`), read-only
- [ ] `initFederation` runs before `import('./bootstrap')`
- [ ] A failed remote load renders a fallback, not a crash
- [ ] The shell's root styles import the design-system theme once

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend.md|platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/solution-host-design-system-consumption.skill.md|solution-host-design-system-consumption]] - [[skills/angular/architecture/v3.1/solutions/solution-host-design-system-consumption.skill/Implementation/platform-shell.federation.extend.md|platform-shell.federation.extend]]

# Unittest TestCases

- [ ] WHEN a remote is added to the runtime manifest THEN the host loads and mounts it with no rebuild
- [ ] WHEN two remotes depend on `@platform/contracts` at compatible versions THEN only one instance (and one Angular) is loaded
- [ ] WHEN a remote is built against an incompatible `@platform/contracts` major THEN the runtime surfaces a version-mismatch error
- [ ] WHEN `RemoteRegistryService.loadRemote` is asked for an unknown remote THEN it rejects with a descriptive error, no unhandled exception
- [ ] WHEN the host session changes THEN every mounted remote sees it through the shared `SESSION_CONTRACT` singleton, no message passing

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]
