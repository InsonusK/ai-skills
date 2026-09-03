---
name: registry-platform-contracts
description: Conflict Detection result for the `platform-contracts` element at plateau-platform-host — the @platform/contracts package, created by solution-platform-contracts and extended by solution-session-sharing with the SessionContract shape
tags:
  - concern/architecture
  - stack/typescript
  - element/platform-contracts
---

# Element
`element/platform-contracts` — the `@platform/contracts` npm package (its own repository): `src/session-contract.ts`, `src/event-bus.ts`, `src/index.ts` — types, interfaces, and DI tokens only.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]] (`.create` — `Repository.create` — the package, the `EventBus` draft interface, the Changesets/publish setup, `@angular/core` as a peer)
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]] (`.extend` — `session-contract.extend` — adds the `SessionContract` interface + `SESSION_CONTRACT` DI token: `currentUser` / `permissions` / `isAuthenticated` as read-only signals)

Both coexist at this plateau — VP2 `SessionSharing` requires the package `solution-platform-contracts` creates.

# Classification
`TMN` — Constraint `T` (VP2 `SessionSharing` `requires` `PlatformContracts`; `solution-session-sharing` declares `depends_on solution-platform-contracts`). Category `M` (a source change — the `.extend` adds `session-contract.ts` + a token to `index.ts`). Kind `N` (independent): the `.extend` adds **one distinct contract shape** to a package built for exactly this; it removes nothing and touches no other shape. The `EventBus` draft (from `.create`) and `SessionContract` (from `.extend`) are member-disjoint.

# Ordering
`source: constraint` — `solution-session-sharing` requires the package, so `solution-platform-contracts` (create) always precedes it. Recorded by `solution-session-sharing`'s `depends_on solution-platform-contracts`.

# Resolution
**Canonical — no resolver.** A contract package extended with more contract shapes is the design intent — the analogue of the `store.config.ts` slice seam and the `LOG_SINKS` sink seam. The `example/` vendors `@platform/contracts` (`SessionContract` + `SESSION_CONTRACT` + `EventBus` draft); the host provides the implementation (`HostSession`), the remote reads it (`requirePermission`), and both share it as a strict federation singleton.

# Architectural signal
N = 2. **Benign.** Not a case for reconsidering VP boundaries — a shared contract package is meant to grow one shape at a time.
