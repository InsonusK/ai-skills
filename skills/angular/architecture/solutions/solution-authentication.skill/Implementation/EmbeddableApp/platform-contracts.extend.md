---
description: Extend the shared @platform/contracts package (from the platform-embeddability solution) so embeddable apps can read the current session and permissions without authenticating on their own
element_kind: repository
change_kind: extend
tags:
  - solution/authentication
  - element/platform-contracts
---

# Structure

No new directories in this monorepo — `@platform/contracts` is published from its own separate repository, per the "Встраиваемость платформы" solution. This extension describes the contract addition conceptually; the actual package's own repository owns the concrete implementation.

## Directory and project skills

| Contract addition | Description |
| ------------------- | ----------- |
| `SessionContract` | A read-only, signal-shaped view of `shared-state`'s auth slice: `currentUser`, `permissions`, `isAuthenticated` — exposed as part of the singleton `@platform/contracts` shared dependency (see the embeddability solution's ADR on singleton sharing). |

# Rules

## MUST
- `@platform/contracts`' `SessionContract` MUST be read-only from an embeddable app's point of view — an embeddable app MUST NOT be able to mutate the session (log in/out, change permissions) through the contract; only the platform's own auth slice does that.
- An embeddable app MUST read session/permission state exclusively through `SessionContract` — it MUST NOT implement its own login flow or maintain its own copy of session state.
- If an embeddable app is loaded without an authenticated session (e.g. `isAuthenticated` is false), it MUST render its own "not authenticated" state rather than attempting its own authentication — redirecting to authenticate is the platform's responsibility, not the embeddable app's.

# Anti-patterns

- **An embeddable app implementing its own login screen "just in case" the platform session is missing**
  - Consequence: duplicates authentication logic across teams, and creates two different ways a user could end up authenticated, defeating the single-session model this solution establishes
  - Instead: the embeddable app only ever reads `SessionContract`; the platform alone is responsible for establishing a session

# Unittest TestCases

- [ ] WHEN an embeddable app reads `SessionContract.permissions` THEN
  - [ ] it reflects the same permission set the platform's own UI uses for its `*hasPermission` checks
- [ ] WHEN the platform's session expires THEN
  - [ ] `SessionContract.isAuthenticated` becomes `false` for every embeddable app reading it, without any action needed on the embeddable app's part
