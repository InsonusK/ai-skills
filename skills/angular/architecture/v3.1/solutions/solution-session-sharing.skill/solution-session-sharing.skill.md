---
name: solution-session-sharing
description: The federation host publishes a live SessionContract implementation through @platform/contracts so every mounted remote reads the same session instance the host reads — the host side of session sharing
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - auth
  - framework/native-federation
  - framework/angular
  - concern/architecture
  - solution/session-sharing

whenToUse: when a federation host needs to expose its authenticated session to mounted remotes, or when reviewing how SessionContract stays in sync with the host's auth slice
creates: []
extends:
  - "@platform/contracts (adds the SessionContract shape + the host's implementation binding)"
  - apps/platform-shell (provides the SessionContract implementation at the composition root)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]"
adr: []
---

# Goal
- Let every remote mounted by this host read the current session (`currentUser`, `permissions`, `isAuthenticated`) as a live, read-only view — without any remote implementing its own authentication.
- Keep that view in lockstep with the host's own `auth` slice, so a login or a session expiry propagates to every remote at once with no polling.

# Capabilities
- Remotes built by separate teams get a working session for free, with no login screen to build or maintain.
- One session model: the same permission strings the host uses for its `*hasPermission` checks are what every remote reads.
- Session expiry reaches every remote simultaneously through the shared singleton — no message passing.

# Core Principle
- `SessionContract` is a **read-only** signal-shaped view of the host's `auth` slice (`solution-authentication`), exposed as part of the singleton `@platform/contracts` shared dependency (`solution-platform-contracts`).
- The host is the sole writer — a remote can never log in/out or change permissions through the contract.
- The implementation is provided once, at `apps/platform-shell`'s composition root, bound to the `auth` slice's selectors.
- A remote loaded with no authenticated session reads `isAuthenticated: false`; establishing a session is the host's responsibility, never the remote's.

# Boundaries
- This is the **host** side. The remote side — a remote *reading* `SessionContract` — is [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/solution-session-consumption.skill.md|solution-session-consumption]] in the `embeddable-app` catalog.
- `platform-host` VP2. Requires the host's monolith has `solution-authentication` (VP7) — a federation host without auth has no session to share.
- Defines only the `SessionContract` shape + the host binding. The broader `@platform/contracts` `EventBus` is `solution-platform-contracts`' concern.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-authentication.skill/solution-authentication.skill.md|solution-authentication]]
  - the `auth` slice whose selectors `SessionContract` is a read-only view of
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]
  - carries the `SessionContract` type as part of the shared singleton package
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]
  - the host that mounts the remotes this session is shared with

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md|@platform/contracts (extend)]] - extend - adds the `SessionContract` shape and the host-side implementation binding

# Workflow

## A remote reads the session (happy path)

1. `apps/platform-shell` provides a `SessionContract` implementation at its root, backed by `selectCurrentUser` / `selectPermissions` / a derived `isAuthenticated`.
2. A mounted remote injects `SessionContract` from `@platform/contracts` — the same singleton instance.
3. On login/logout/expiry the host's `auth` slice changes once; every remote's view updates reactively.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/Implementation/session-contract.extend.md#MUST|session-contract.extend]]
- `SessionContract` is read-only from a remote's point of view — the host's `auth` slice is the only writer.
  - Risk: two ways to establish a session; the single-session model breaks.
  - Fix: the contract exposes selectors only, no dispatch.
- The implementation is provided exactly once, at `apps/platform-shell`'s composition root.
  - Risk: multiple bindings drift; a remote sees stale session state.
  - Fix: one `provide`, bound to the `auth` selectors.

## SHOULD
- Avoid deriving `isAuthenticated` independently in each consumer — expose it from the contract so the rule is defined once.

# Check list
- [ ] `SessionContract` is exposed through the singleton `@platform/contracts`, read-only.
- [ ] Its implementation is bound to the host's `auth` slice selectors, provided once at the shell root.
- [ ] A session expiry in the host flips `isAuthenticated` for every remote with no remote-side action.
