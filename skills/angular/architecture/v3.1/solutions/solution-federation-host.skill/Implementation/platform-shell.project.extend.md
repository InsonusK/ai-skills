---
description: Turn platform-shell into a Native Federation dynamic host that discovers and loads embeddable apps at runtime
name: platform-shell
project_kind: application
element_kind: project
change_kind: extend
tags:
  - solution/federation-host
  - element/platform-shell-project
---

# Goals

- Load independently built and deployed embeddable apps into the platform shell at runtime, without rebuilding the shell
- Share a single Angular runtime and a single instance of `@platform/contracts` between the host and every loaded embeddable app

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    federation.config.ts
    /app
      /remote-registry
        remote-registry.service.ts
      /shell
        shell.component.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| federation.config.ts | Native Federation config: declares `@platform/contracts` (and Angular) as `singleton: true` shared dependencies; host does not declare any remotes at build time (Dynamic Federation) |
| /remote-registry/remote-registry.service.ts | Runtime service that resolves the list of available embeddable apps and their `remoteEntry` URLs from configuration (see [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create]]) |
| /shell/shell.component.ts | Mounts a resolved remote's exposed entry point into a host-provided slot, using `loadRemoteModule` |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor | Federation build plugin and `loadRemoteModule` runtime API |
| @platform/contracts | semver range | Shared EventBus/state contract, declared as singleton |

# Rules

## MUST
- `federation.config.ts` must never list embeddable apps' remote entries statically — they are resolved by `remote-registry.service.ts` at runtime.
- Any component or service in `platform-shell` that needs to talk to an embedded app must do so exclusively through the `EventBus`/state contracts exposed by `@platform/contracts` — never by reaching into a remote's internals.

## SHOULD
- `remote-registry.service.ts` should treat a failure to resolve or load a given remote as a recoverable error (show a fallback UI slot), not a fatal error for the whole shell.

- **Reaching into a loaded remote's internal exports instead of the shared contract** — Consequence: couples the shell to one remote's internal structure, breaking silently when that remote refactors — Instead: all cross-boundary interaction goes through `@platform/contracts`
# Check list

- [ ] `federation.config.ts` marks `@platform/contracts` and Angular as `singleton: true`
- [ ] No embeddable app's remote entry is hardcoded in `platform-shell`'s source or build config
- [ ] Failure to load a given remote does not crash the rest of the shell
