---
name: facade-client-layering
description: How feature-level data operations are structured inside data-access libs, and whether classical NgRx (action/reducer/effect) is still used at the feature level
problem: The originally used flow (Facade dispatches an Action, an Effect calls a Client that does HTTP+DTO mapping) duplicates orchestration already provided by the NgRx Signal Store adopted for feature-level state in the "State management" solution
decision: Collapse Action/Reducer/Effect for feature-level operations; the feature's Signal Store method calls a Facade directly, which calls an internal Client
tags:
  - solution/api-http-layer
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The pattern in use before this solution was: UI calls a **Facade** that validates input and dispatches an **Action**; a **Reducer** updates state; an **Effect** catches the action and calls a **Client**, which maps to/from DTOs, calls `HttpClient`, and throws its own error. This predates the "State management" solution's decision to use NgRx Signal Store (not classical NgRx) for feature-level state. Once a feature's state lives in a Signal Store, the store's own methods already provide "receive a command, orchestrate work, update state" — the exact role the Action/Reducer/Effect triad used to play. Keeping both is redundant: two different mechanisms end up doing the same job for the same kind of state. We need to decide whether to keep the classical NgRx chain for feature-level operations, or let it collapse now that Signal Store owns that role, and — separately — clarify what "Facade" and "Client" each mean, since the same name was being used for two different responsibilities in different parts of the architecture so far.

# Selected variant

**Selected variant:** [[#Signal Store calls Facade directly; Client is an internal transport detail]]

For feature-level operations, the Signal Store method itself (see the "State management" solution) is the orchestration point: it calls a **Facade** (business validation/orchestration, public API of the `data-access` lib) which calls an internal **Client** (DTO mapping + `HttpClient` + error normalization, not exported outside the `data-access` lib). No Action, Reducer, or Effect is introduced for feature-scoped operations. Classical NgRx (action/reducer/effect calling the same Facade→Client layering) remains exactly as already established for global/cross-cutting state (auth, notifications, offline-sync) in the "State management" and `solution-authentication`s — this decision does not change that.

# Searched variants

## Signal Store calls Facade directly; Client is an internal transport detail

### Description

- **Facade**: the `data-access` lib's public API. Contains business-rule validation and orchestration (may call more than one Client method, may enrich a request with data read from `shared-state`). Exceptions/rejections it surfaces are always the Client's typed domain errors, optionally wrapped with additional business context.
- **Client**: internal to the `data-access` lib, never exported from its `index.ts`. Maps domain model ↔ DTO, calls the shared base HTTP service, and is the single place a raw `HttpErrorResponse` is caught and converted into a typed domain error.
- A feature's Signal Store method calls the Facade directly (`async addOrder() { ... await this.ordersFacade.addOrder(...) ... }`), and updates its own state (`loading`, data, `error`) via `patchState` based on the result — no Action/Reducer/Effect involved.

### Benefits

- Removes a layer of indirection that no longer serves a purpose once Signal Store owns feature-level state orchestration — one mechanism for "receive command → do work → update state," not two competing ones
- Matches the layering already used (if inconsistently named) in the "State management" and `solution-authentication`s' code examples — no architectural rewrite of those solutions is required, only a naming clarification
- Business validation (Facade) stays clearly separated from transport/mapping (Client), preserving the useful part of the original design
- Global/cross-cutting state keeps its classical NgRx chain exactly as already decided, so the audit-log/effect-based-retry benefits chosen in "State management" are unaffected

### Costs

- Requires renaming/re-labeling the existing code examples in the "State management" and `solution-authentication`s' Facade references to make clear they call an internal Client — a documentation clarification, not a functional rewrite
- Anyone previously familiar with the original Facade→Action→Reducer→Effect→Client flow needs to unlearn it for feature-level operations specifically (it remains correct for global state)

## Keep Action/Reducer/Effect at the feature level too

### Description

Every feature-level operation continues to go through Facade → Action → Reducer → Effect → Client, exactly as originally designed, in addition to whatever Signal Store also does.

### Benefits

- No change from the original design; team already has some familiarity with it
- A single, uniform mechanism (classical NgRx) for every kind of state change, feature-level or global

### Costs

- Directly contradicts the "State management" solution's decision to use Signal Store (not classical NgRx) for feature-level state — either that solution's premise was wrong, or this pattern duplicates it
- Two mechanisms end up doing the same orchestration job for the same state, multiplying boilerplate (an action creator, a reducer case, an effect) for what a single Signal Store method already does in one place
- Makes the codebase inconsistent with the Signal Store pattern already documented with concrete examples in "State management"

## Merge Facade and Client into a single class (no internal split)

### Description

Drop the Facade/Client distinction entirely; a single `data-access` service does validation, DTO mapping, and the HTTP call all in one place.

### Benefits

- Fewer files/classes per feature — less initial scaffolding for a very small feature
- No ambiguity about which method belongs in which layer

### Costs

- Business validation and transport/mapping concerns end up mixed in the same class, which is exactly the coupling the original two-layer design (Facade doing business logic, Client doing transport) was set up to avoid
- Harder to unit test business rules in isolation from HTTP/DTO concerns, since they live in the same class
- Loses the reusable internal Client shape that the future `solution-offline-sync` is likely to need to intercept independently of business validation
