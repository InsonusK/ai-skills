---
description: Baseline structure required of any independent repository that hosts an embeddable application consumed by the platform
element_kind: repository
change_kind: create
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
- The repository MUST publish its `remoteEntry` and exposed module path to a location the platform's runtime remote registry can discover (see [[../PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create.md]]) — how that publication happens (a manifest file, a registration API call, etc.) is an operational detail owned by this repository's own deploy pipeline.
- The repository MUST declare `@platform/contracts` as a `singleton: true` shared dependency in its federation config, at a version compatible with the platform's expected range.
- The repository MUST NOT import platform-shell internals directly — the only contract with the platform is `@platform/contracts` plus the federation `remoteEntry`/exposed-module boundary.

## SHOULD
- The repository SHOULD run its own CI pipeline (lint/test/build/deploy) independent of the platform's pipeline — this is the entire point of the independent-deploy requirement behind this solution.

# Anti-patterns

- **Bumping `@platform/contracts` to an incompatible major version without coordinating with the platform team**
  - Consequence: the federation runtime will either duplicate the shared singleton or fail to load the remote entirely, depending on `strictVersion` configuration
  - Instead: treat `@platform/contracts`' version range as a cross-team contract; coordinate major-version bumps the same way any published API's breaking change would be coordinated

- **Depending on platform-shell's own package instead of only `@platform/contracts`**
  - Consequence: recreates tight coupling to the platform's internals that federation was chosen specifically to avoid
  - Instead: everything the embeddable app needs from the platform must be expressed in `@platform/contracts`

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
