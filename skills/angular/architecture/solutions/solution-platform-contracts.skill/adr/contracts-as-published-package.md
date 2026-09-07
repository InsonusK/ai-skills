---
name: contracts-as-published-package
description: Why @platform/contracts is its own repository published to npm with strictVersion sharing, not a monorepo lib
problem: The host and every remote need one build-time contract. It could be a lib in the platform monorepo, a loosely-shared package, or a strictly-versioned published package.
decision: @platform/contracts is its own repository, built and published to npm like any external dependency (the same pattern as the design-system package), and shared singleton:true strictVersion:true by every consumer.
tags:
  - solution/platform-contracts
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

A federation host and every independently deployed remote need exactly one build-time contract so neither imports the other's internals. Three shapes are possible: a `libs/shared/contracts` in the platform monorepo, a loosely-shared npm package (`strictVersion: false`), or a strictly-versioned published package.

# Selected variant

**Selected variant:** [[#Own repository, published to npm, strictVersion sharing (selected)]]

# Searched variants

## Own repository, published to npm, strictVersion sharing (selected)

### Description
`@platform/contracts` lives in its own repository, versioned by Changesets, published to npm. Host and every remote declare it `singleton: true`, `strictVersion: true`.

### Benefits
- A remote team consumes it like any external dependency — no coupling to the platform monorepo, no monorepo checkout.
- `strictVersion: true` makes an incompatible major a visible load-time failure, never a silently duplicated contract instance that breaks state sharing.
- Independent release cadence; a contract change is a deliberate semver event.

### Costs
- One more repository and publish pipeline to maintain. Acceptable — the package is tiny (types only) and changes rarely.

## A libs/shared/contracts lib in the platform monorepo

### Description
The contract is a normal Nx lib; remotes depend on it via a published snapshot or a git dependency.

### Costs
- Remote teams must track the platform monorepo's versioning and release cadence — the coupling this whole architecture exists to avoid.
- No clean semver boundary; "what version of the contract" becomes "what platform commit".

## Loosely-shared package (strictVersion: false)

### Description
Same as selected, but `strictVersion: false`.

### Costs
- An incompatible major loads a second, non-shared instance of the contract package — state sharing silently breaks with no error. The failure mode is invisible until a user hits it.
