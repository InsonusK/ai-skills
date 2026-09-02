---
name: solution-session-consumption
description: A federation remote reads SessionContract (currentUser, permissions, isAuthenticated) from @platform/contracts and gates its own UI on it — never implementing its own login flow or keeping its own session copy
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
  - solution/session-consumption

whenToUse: when a remote needs to show user-scoped data or gate UI on permissions, or when reviewing why a remote sees isAuthenticated false
creates: []
extends:
  - "the remote's exposed module (reads SessionContract, renders a not-authenticated state)"
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]"
adr: []
---

# Goal
- Let a remote show user-scoped data and gate UI on permissions using the host's session, with zero authentication code of its own.
- Keep authorization expressed the same way as the host — permission strings, never role names.

# Capabilities
- A remote team ships a permission-aware UI without building or maintaining any auth flow.
- One session model across host and every remote — a permission check means the same thing everywhere.

# Core Principle
- The remote reads `SessionContract` (`currentUser`, `permissions`, `isAuthenticated`) from `@platform/contracts` — the same singleton the host published via `solution-session-sharing`.
- The remote never implements a login flow and never keeps its own copy of session state.
- If `isAuthenticated` is `false` the remote renders a "not authenticated" state and defers to the host — it never redirects to authenticate.
- Authorization checks the remote makes are permission strings, never role names.

# Boundaries
- This is the **remote** side. The host side (publishing `SessionContract`) is [[skills/angular/architecture/v3.1/solutions/solution-session-sharing.skill/solution-session-sharing.skill.md|solution-session-sharing]].
- `embeddable-app` VP1. No legality gate on the host: a remote configured for session consumption with no host `SessionSharing` simply reads `isAuthenticated: false` forever — it is *meaningful* only when a host publishes.
- A remote with no user-scoped data at all (a public widget) does not compose this solution.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]
  - the baseline remote contract this reads the session on top of
- [[skills/angular/architecture/v3.1/solutions/solution-platform-contracts.skill/solution-platform-contracts.skill.md|solution-platform-contracts]]
  - carries the `SessionContract` shape the remote reads

# Template Skill Mutations

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md|Exposed module (generic pattern)]] - extend - inject `SessionContract`, render a not-authenticated state, express authorization as permission strings

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-session-consumption.skill/Implementation/session-consumption.extend.md#MUST|session-consumption.extend]]
- Never implement a login flow or keep a local copy of session state in the remote.
  - Risk: two sources of truth; a user authenticated in the remote but not the host (or vice versa).
  - Fix: read `SessionContract` only; the host owns the session.
- Never redirect to authenticate when `isAuthenticated` is false — render a not-authenticated state and defer to the host.
- Never check a role name — authorization is permission strings, matching the host.

# Check list
- [ ] The remote reads session state only through `SessionContract`.
- [ ] No login flow, no local session copy in the remote.
- [ ] `isAuthenticated: false` renders a not-authenticated state, no redirect.
- [ ] Authorization checks are permission strings.
