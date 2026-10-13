---
description: Extend the shared @platform/contracts package (from solution-platform-contracts) so embeddable apps can read the current session and permissions without authenticating on their own
element_kind: repository
change_kind: extend
tags:
  - solution/session-sharing
  - element/platform-contracts
---

# Structure

No new directories in this monorepo — `@platform/contracts` is published from its own separate repository, per `solution-platform-contracts`. This extension describes the contract addition conceptually; the actual package's own repository owns the concrete implementation.

## Directory and project skills

| Contract addition | Description |
| ------------------- | ----------- |
| `SessionContract` | A read-only, signal-shaped view of `shared-state`'s auth slice: `currentUser`, `permissions`, `isAuthenticated` — exposed as part of the singleton `@platform/contracts` shared dependency (see solution-platform-contracts). |

# Rules

## MUST
- `SessionContract` is read-only from an embeddable app's point of view — no method mutates the session (login/logout, permission change).
  - Risk: a writable contract lets a remote change auth state the platform owns, creating races and an inconsistent session across apps.
  - Fix: expose `currentUser`/`permissions`/`isAuthenticated` as readonly signals; mutation lives only in the platform's auth slice.
- An embeddable app reads session/permission state only through `SessionContract` — never its own login flow or its own copy of session state.
  - Risk: a second source of session truth drifts from the platform's and produces two ways a user can be "authenticated".
  - Fix: inject `SESSION_CONTRACT`; derive all gating from it.
- Loaded without an authenticated session, an embeddable app renders its own "not authenticated" state — it never triggers authentication.
  - Risk: a remote initiating a redirect-to-login fights the platform's own auth handling and can loop or land the user in the wrong place.
  - Fix: when `isAuthenticated()` is false, show a passive empty/blocked state; the platform decides whether to redirect.

# Unittest TestCases

- [ ] WHEN an embeddable app reads `SessionContract.permissions` THEN
  - [ ] it reflects the same permission set the platform's own UI uses for its `*hasPermission` checks
- [ ] WHEN the platform's session expires THEN
  - [ ] `SessionContract.isAuthenticated` becomes `false` for every embeddable app reading it, without any action needed on the embeddable app's part

## SHOULD
- **An embeddable app implementing its own login screen "just in case" the platform session is missing** — Consequence: duplicates authentication logic across teams, and creates two different ways a user could end up authenticated, defeating the single-session model this solution establishes — Instead: the embeddable app only ever reads `SessionContract`; the platform alone is responsible for establishing a session
