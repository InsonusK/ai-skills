---
name: solution-platform-contracts
description: The @platform/contracts package — an independently versioned, independently published npm package that is the sole build-time contract between a federation host and its remotes; carries the EventBus/shared-state contract and the SessionContract shape, no implementation
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - framework/native-federation
  - framework/angular
  - concern/architecture
  - solution/platform-contracts

whenToUse: when setting up the @platform/contracts package, adding a contract shape (an EventBus channel, a shared-state interface), or reviewing why host and remote disagree on a contract version
creates:
  - "@platform/contracts (its own repository, published to npm)"
extends: []
depends_on: []
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/adr/contracts-as-published-package.md|contracts-as-published-package]]"
---

# Goal
- Give a federation host and every remote **one** build-time contract — a small, typed npm package — so neither side ever imports the other's internals.
- Make cross-team compatibility a semver contract, not a monorepo implementation detail.
- Carry contract *shapes* only (the `EventBus` channel types, the shared-state interfaces, the `SessionContract` shape) — never an implementation.

# Capabilities
- Independent release cadence for the contract, decoupled from both the host's and every remote's own schedule.
- A single, greppable place any team reads to know what the host↔remote boundary is.
- `strictVersion: true` federation sharing makes an incompatible major version a visible load-time failure, not a silently duplicated runtime.

# Core Principle
- `@platform/contracts` lives in its **own repository**, built and published like any external dependency — the same pattern this architecture uses for the `design-system` package.
- It exports **types and tokens only**: `EventBus` channel interfaces, shared-state interfaces, and the `SessionContract` shape (`currentUser`, `permissions`, `isAuthenticated`). Zero runtime logic.
- Host and every remote declare it `singleton: true`, `strictVersion: true` in their federation config.
- A change to an exported shape ships a semver bump (a changeset, like the design system): a removed/renamed field is a major.

# Boundaries
- This solution defines the package, not its consumers. The host shares it as a singleton (`solution-federation-host`); each remote declares it as a singleton (`solution-federation-remote`); `SessionContract` is *implemented and published* by `solution-session-sharing` (host) and *read* by `solution-session-consumption` (remote).
- The `EventBus` beyond the `SessionContract` is a draft — only the session shape is worked out in this catalog. Typed event channels are the aspirational `ContractEventBus`.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/adr/contracts-as-published-package.md|contracts-as-published-package]] — its own repo + npm publish + `strictVersion: true`, over a monorepo lib or a loosely-shared package. Rejected: a `libs/shared/contracts` in the platform monorepo (couples remote teams to the monorepo); `strictVersion: false` (a silent duplicate contract instance breaks state sharing invisibly).

# Requirements

NPM:
- `@angular-architects/native-federation` — the consumers declare this package as a shared singleton via its config.

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository]] - create - the `@platform/contracts` package repository: TypeScript library, Changesets, published to npm, exporting contract types + tokens only

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md#MUST|Repository.create]]
- Never put runtime logic in `@platform/contracts` — types, interfaces, and DI tokens only.
  - Risk: a behavioural change forces every consumer to re-test; the package stops being a pure contract.
  - Fix: implementations live in the host (`solution-session-sharing`) or a remote; the package only declares shapes.
- Never share `@platform/contracts` with `strictVersion: false`.
  - Risk: an incompatible major loads a second, non-shared instance and state sharing silently breaks.
  - Fix: `strictVersion: true` everywhere — a mismatch is a visible failure to fix.

## SHOULD
- Avoid adding a field to a shared interface without a changeset classifying the bump.

# Check list
- [ ] `@platform/contracts` is its own repository, published to npm, versioned by Changesets.
- [ ] It exports only types, interfaces, and DI tokens — no runtime code.
- [ ] Host and every remote declare it `singleton: true`, `strictVersion: true`.
