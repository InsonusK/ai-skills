---
name: solution-federation-remote
description: The baseline any independently deployed remote app must satisfy to be loadable by a federation host — a Native Federation remoteEntry, an exposed module, singleton Angular + @platform/contracts, hierarchical route ownership one level down, and an independent CI/CD pipeline; imposes no internal architecture
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - framework/native-federation
  - framework/angular
  - concern/architecture
  - solution/federation-remote

whenToUse: when scaffolding a new embeddable app repository, exposing its module for federation, or reviewing whether it satisfies the host contract
creates:
  - "{embeddable-app-name} (its own repository)"
extends: []
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/adr/embedding-mechanism.md|embedding-mechanism]]"
---

# Goal
- Define the fixed contract a remote must satisfy to be mounted by a `solution-federation-host` platform — and nothing more.
- Keep a remote repository free to choose its own tooling, structure, state, and tests — only the federation boundary is prescribed.
- Reuse `solution-app-routing`'s hierarchical route ownership one level down, so a remote never hardcodes its own mount prefix.

# Capabilities
- Any team, any workspace tool (Nx not required), can ship a mountable remote by satisfying one small contract.
- Independent CI/CD — a remote builds, tests, and deploys on its own schedule.
- A remote is mounted by the host exactly like a local feature: one root segment, resolved from the manifest.

# Core Principle
- The remote exposes a Native Federation `remoteEntry` and one exposed module (typically `./Module`).
- Angular and `@platform/contracts` are declared `singleton: true`, `strictVersion: true` — host and remote run one runtime, one contract instance.
- Inside the exposed module, the remote mounts its own feature root segments using the hierarchical pattern from `solution-app-routing` — relative paths only, never its own mount prefix.
- **No import of `platform-shell` internals in either direction** — the only contract is `@platform/contracts` + the federation boundary.
- The remote's own internal architecture is unconstrained. A remote *may* adopt the `monolith` catalog's feature models internally (the aspirational `RemoteInternalArchitecture`), but is not required to.

# Boundaries
- This solution is the contract, not a build guide for the remote's insides. It adds no `feature`/`data-access` split, no state tier, no test setup.
- `RemoteSessionConsumption` (reading `SessionContract`) and `RemoteDesignSystemConsumption` (the shared design system) are separate, optional remote solutions — a minimal remote has neither.
- `parent_plateaus` is empty for this catalog's plateaus — a remote is not a continuation of the platform chain. (A `RemoteInternalArchitecture=Yes` plateau is the one exception; it composes a `monolith` plateau.)

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/adr/embedding-mechanism.md|embedding-mechanism]] — shared with the host: Native Federation + Dynamic Federation is the mechanism both sides implement.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]
  - the hierarchical route-ownership pattern the exposed module reuses one level down
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]
  - provides `@platform/contracts`, declared here as a strict shared singleton

NPM:
- `@angular-architects/native-federation` — federation build plugin + `loadRemoteModule` runtime API.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md|Repository]] - create - the baseline remote repository: federation remote config, `@platform/contracts` singleton, exposed module, independent CI/deploy

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|Exposed module (generic pattern)]] - extend - mounts its own features' root segments, reusing `solution-app-routing`'s hierarchical ownership one level down

# Workflow

## Scaffold a mountable remote (happy path)

1. The team creates a repo (any tooling) with a Native Federation config: `name`, `exposes: { './Module': ... }`, `remoteEntry`.
2. Angular + `@platform/contracts` declared `singleton: true`, `strictVersion: true`.
3. The exposed module defines its own root-relative routes — no mount prefix baked in.
4. CI/CD builds and deploys the remote independently; the `remoteEntry` URL is registered in the platform's manifest.

## Version-incompatible contract (failure path)

1. The remote is built against a `@platform/contracts` major incompatible with the host's.
2. `strictVersion: true` makes the federation runtime refuse to load the remote with a version-mismatch error — visible, not a silent duplicate.
3. Fix: align the remote's `@platform/contracts` range and redeploy.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md#MUST|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md#MUST|routes.ts.extend]]
- Never hardcode the remote's own expected mount prefix in its routes.
  - Risk: the remote cannot be remounted at a different segment without a code change.
  - Fix: relative paths only; the host assigns the segment at mount time.
- Never import `platform-shell` internals, and never let a remote export anything the host imports beyond the exposed module.
  - Risk: build-time coupling that breaks independent deployability.
  - Fix: the only contract is `@platform/contracts` + the federation boundary.

## SHOULD
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/Repository.create.md#SHOULD|Repository.create]]
- Avoid bumping `@platform/contracts` to an incompatible major without cross-team coordination.

# Check list
- [ ] The remote exposes a valid `remoteEntry` and one exposed module.
- [ ] Angular and `@platform/contracts` are `singleton: true`, `strictVersion: true`.
- [ ] The exposed module mounts its own features with relative paths, no mount prefix baked in.
- [ ] No import between the remote's internals and `platform-shell`'s internals.
- [ ] The remote builds, tests, and deploys on its own pipeline.
