---
description: Baseline structure required of any independent repository that hosts an embeddable application consumed by the platform
element_kind: repository
change_kind: create
tags:
  - solution/federation-remote
  - element/embeddable-repository
---

# Structure

## Workspace Structure

This is a separate repository from the platform monorepo, owned and deployed independently by its own team. It is not required to be an Nx workspace — a plain Angular CLI workspace is sufficient, since this repo hosts a single deployable unit and does not need affected-based builds or cross-project boundary enforcement the way the platform monorepo does.

```
/{embeddable-app-name}
  /src
    federation.config.ts
    /app
      app.component.ts
  package.json
```

## Directory and project skills

| Directory/file | Description |
| --------------- | ----------- |
| federation.config.ts | Native Federation remote config: exposes one or more entry points (typically a top-level component), declares `@platform/contracts` (and Angular) as `singleton: true`, matching the version range the platform host expects |
| package.json | Declares a dependency on `@platform/contracts` at a version compatible with the platform's expected range — this is the versioning contract between the two independently deployed codebases |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular-architects/native-federation | pinned minor, matching the Angular major version in use | Federation remote build plugin |
| @platform/contracts | semver range compatible with the platform's declared range | Shared EventBus/state contract; MUST be the same singleton contract the platform host shares |

# Rules

## MUST
- The repository publishes its `remoteEntry` and exposed module path to a location the platform's runtime remote registry can discover.
  - Risk: if the platform cannot discover the entry, the app can only be wired in by a host code change, defeating independent deploy.
  - Fix: this repo's deploy pipeline writes the manifest entry (or calls the registration API); see [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create|remote-registry.service.ts]].
- The repository declares `@platform/contracts` as a `singleton: true` shared dependency at a version compatible with the platform's range.
  - Risk: a non-singleton or incompatible version loads a second contracts instance, breaking `EventBus` and shared-type identity across the boundary.
  - Fix: `shared: { '@platform/contracts': { singleton: true }, ... }`; treat the version range as a cross-team contract.
- The repository never imports platform-shell internals — the only contract is `@platform/contracts` plus the `remoteEntry`/exposed-module boundary.
  - Risk: importing the host's package recreates the tight coupling federation was chosen to avoid and breaks on any host refactor.
  - Fix: everything the app needs from the platform is expressed in `@platform/contracts`.
## SHOULD
- The repository should run its own CI pipeline (lint/test/build/deploy) independent of the platform's pipeline — this is the entire point of the independent-deploy requirement behind this solution.

- **Bumping `@platform/contracts` to an incompatible major version without coordinating with the platform team** — Consequence: the federation runtime will either duplicate the shared singleton or fail to load the remote entirely, depending on `strictVersion` configuration — Instead: treat `@platform/contracts`' version range as a cross-team contract; coordinate major-version bumps the same way any published API's breaking change would be coordinated
- **Depending on platform-shell's own package instead of only `@platform/contracts`** — Consequence: recreates tight coupling to the platform's internals that federation was chosen specifically to avoid — Instead: everything the embeddable app needs from the platform must be expressed in `@platform/contracts`
# Check list

- [ ] Repository builds and deploys independently, with its own CI pipeline
- [ ] `federation.config.ts` declares `@platform/contracts` (and Angular) as `singleton: true`
- [ ] The exposed `remoteEntry`/module is discoverable by the platform's runtime remote registry
- [ ] No import of platform-shell internals exists anywhere in the repository

# Unittest TestCases

- [ ] WHEN the embeddable app is loaded standalone (outside the platform, e.g. for local development) THEN
  - [ ] it still renders correctly, since it does not depend on platform-shell internals
- [ ] WHEN the platform host loads this app's remoteEntry THEN
  - [ ] the exposed component mounts and can send/receive events through `@platform/contracts`' EventBus
