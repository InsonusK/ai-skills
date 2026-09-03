---
name: plateau-platform-host--repo-platform-contracts
description: The @platform/contracts package — its own repository, published to npm, exporting the SessionContract shape + SESSION_CONTRACT DI token + the draft EventBus interface; types and tokens only, no implementation, no Angular compilation — platform-host plateau
domain: skill
type: template
whenToUse: when setting up or changing the @platform/contracts package — adding a contract shape (a SessionContract field, an EventBus channel), configuring its build/publish, or reviewing why host and remote disagree on a contract version
plateau: platform-host
project_kind: library
version: 20260903180000
tags:
  - skill/template/repo
  - plateau/platform-host
  - stack/typescript
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]]"
---

> `@platform/contracts` is its **own repository**, separate from the platform monorepo and every remote — built and published like any external dependency, the same pattern as the `design-system` package. Owned by the platform-host team, consumed by every remote.

# Goal

- Give the federation host and every remote **one** build-time contract — a small typed npm package — so neither side ever imports the other's internals
- Make cross-team compatibility a semver contract, not a monorepo implementation detail

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]

# Structure

```
@platform-contracts/            (its own repo, published as "@platform/contracts")
  package.json                  type: module; exports types + tokens; @angular/core is a PEER only
  tsconfig.json                 rootDir: src; explicit .js import extensions in source
  .changeset/config.json        every public-surface change ships a changeset; a removed/renamed field is a major
  src/
    session-contract.ts         SessionContract interface + SESSION_CONTRACT InjectionToken + SessionUser
    event-bus.ts                EventBus channel interface (draft — only SessionContract is worked out)
    index.ts                    public API — types, interfaces, DI tokens ONLY
```

## SessionContract shape

```typescript
export interface SessionContract {
  readonly currentUser: Signal<SessionUser | null>;
  readonly permissions: Signal<readonly string[]>;   // permission strings, never role names
  readonly isAuthenticated: Signal<boolean>;
}
export const SESSION_CONTRACT = new InjectionToken<SessionContract>('platform.SessionContract');
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] - [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|session-contract.extend]]

# Rules

## MUST
- Export types, interfaces, and DI tokens **only** — never a runtime implementation. Implementations live in the host (`HostSession`) or a remote.
- Publish to npm with a Changeset classifying every bump; a removed or renamed exported field is a **major**.
- The build produces a plain ESM + `.d.ts` library — no Angular compilation, no framework dependency. `@angular/core` is a **peerDependency** (never a regular dependency), so it resolves to the single shared instance in host and remote.
- `SessionContract` is read-only — nothing in the package lets a consumer mutate a session.
- Ship ESM with explicit `.js` import extensions in source and a `rootDir` — a bare bundler-resolution build emits extensionless specifiers a Node/Vite consumer refuses.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]

# Check list

- [ ] The repository is separate from the platform monorepo and any remote
- [ ] `src/index.ts` exports only types, interfaces, and DI tokens
- [ ] `@angular/core` is a `peerDependency`, not a `dependency`
- [ ] Every PR touching the public surface ships a changeset
- [ ] The build is plain ESM + `.d.ts`, no Angular compilation

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]

# Unittest TestCases

- [ ] WHEN `src/index.ts` is inspected THEN it exports no `class`/`function` with a body — only `interface` / `type` / `const token`
- [ ] WHEN host and remote both depend on a compatible `@platform/contracts` version THEN the federation runtime loads one instance
- [ ] WHEN a remote is built against an incompatible major THEN `strictVersion: true` makes it a visible load-time failure

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] - [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/Implementation/Repository.create.md|Repository.create]]
