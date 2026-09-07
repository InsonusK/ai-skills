---
name: session-contract-ownership
description: Where SessionContract publication lives and which way the auth↔federation dependency points
problem: V1 bundled "authenticate the app", "publish a session to embedded remotes", and "be a federation host" into solution-platform-embeddability, with solution-authentication depending on it — so a plain authenticated monolith could not exist without the federation layer, and the dependency pointed from the lower layer (auth) to the higher one (federation)
decision: SessionContract publication is its own solution, solution-session-sharing, in the platform-host catalog; it depends_on the monolith solution-authentication (plus solution-platform-contracts and solution-federation-host). Monolith solution-authentication has no federation dependency of any kind
tags:
  - solution/session-sharing
  - stack/typescript
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

V1's `solution-platform-embeddability` was three concerns in one skill: turn the shell into a federation host, define `@platform/contracts`, and publish a live `SessionContract` implementation for mounted remotes to read. V1's `solution-authentication` then declared `depends_on solution-platform-embeddability`.

Two things are wrong with that once `monolith/` and `platform-host/` are separate catalogs (feature-model open question 3):

- **A monolith that is not a platform host still needs authentication.** With the V1 edge, composing auth drags in the entire federation layer.
- **The dependency points the wrong way.** Authentication is a monolith-tier concern; publishing a session to remotes is a federation-tier concern built *on top of* auth. The edge `auth → federation` inverts the layering.

The question: which unit owns `SessionContract` publication, and how is it wired to the auth slice it reflects?

# Selected variant

**Selected variant:** [[#A separate platform-host solution that depends on monolith auth (selected)]]

`solution-session-sharing` lives in `platform-host/`, realizes `platform-host` VP2, and `depends_on` the monolith `solution-authentication` (the `auth` slice it exposes a read-only view of), `solution-platform-contracts` (the package the contract type ships in), and `solution-federation-host` (the host that mounts the remotes). Monolith `solution-authentication` was rewritten with no federation dependency — it produces the `auth` slice and stops there.

# Searched variants

## A separate platform-host solution that depends on monolith auth (selected)

### Description

`SessionContract` publication is carved out of V1 `solution-authentication` into a new `solution-session-sharing` in the `platform-host` catalog. It provides one read-only `SessionContract` implementation at `apps/platform-shell`'s composition root, bound to the monolith `auth` slice's selectors, exposed through the singleton `@platform/contracts`. The dependency edge runs `session-sharing → authentication`, low-tier to nothing, high-tier to low.

### Benefits

- A plain authenticated monolith (`plateau-multiuser-monolith`) composes `solution-authentication` with no federation code anywhere in its graph.
- Layering is honest: `session-sharing` (federation tier) builds on `authentication` (monolith tier), never the reverse.
- The carve-out is a clean seam — `solution-authentication`'s output is the `auth` slice; `solution-session-sharing`'s input is that slice's selectors. Neither knows the other's internals.
- `embeddable-app`'s `solution-session-consumption` depends on `solution-platform-contracts` only — it reads the contract shape, never the host implementation.

### Costs

- One more solution in the catalog, and a reader tracing "how does a remote get the session" follows two hops (`session-consumption` → `platform-contracts` ← `session-sharing` → `authentication`) instead of one skill.
- The `auth` slice is now a cross-catalog dependency target — a breaking change to its selectors ripples into `platform-host/`. Acceptable: those selectors are a small, stable surface (`selectCurrentUser`, `selectPermissions`, a derived `isAuthenticated`) and the coupling is exactly what "share the host's session" means.

## Keep it in solution-authentication with an optional "publish" mode

### Description

`solution-authentication` stays one skill covering both the `auth` slice and an optional `SessionContract` publication step, gated on whether the app is a federation host.

### Benefits

- One skill to read for everything session-related.
- No new solution, no extra `depends_on` hops.

### Costs

- `solution-authentication` carries a federation concept (`@platform/contracts`, remote consumers) that a non-host monolith never uses — the skill can no longer be read as "monolith auth".
- The optional mode still needs `@platform/contracts` available, so the skill either `depends_on solution-platform-contracts` unconditionally (a monolith depending on a federation package) or describes a dependency it does not declare.
- `platform-host` VP2 would have no realizing solution of its own — the Variability Map would point `Realized by` at "the publish mode of `solution-authentication`", which the pipeline discourages.

## Keep the V1 direction (authentication depends_on the federation layer)

### Description

Leave `solution-authentication depends_on solution-platform-embeddability` (or its v3.1 successor) as V1 had it.

### Benefits

- No migration work on the dependency edges.

### Costs

- Authentication cannot be composed without the federation layer — there is no authenticated-monolith plateau, which is a combination `monolith/` explicitly wants (`plateau-multiuser-monolith`).
- The inverted edge means `nx affected` / skill-graph tooling treats every federation change as touching auth.
- Contradicts the feature model, where `Authentication` is a `monolith/` feature and `SessionSharing` is a separate `platform-host/` feature that *requires* it.
