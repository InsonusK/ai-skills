---
name: state-management-tiering
description: Choice of state management approach across the Angular application
problem: Where should different kinds of application state live — a single uniform tool for everything, or different tools for different scopes of state
decision: Use a three-tier policy — Angular Signals for component-local state, NgRx Signal Store for feature-level state, classical NgRx Store for global/cross-cutting state
tags:
  - solution/state-management
  - concern/documentation
  - concern/documentation/adr
---

# Problem

State in the application ranges from purely local UI concerns (is a dialog open) to feature-scoped data (the list of orders currently being edited) to genuinely global, cross-cutting concerns (auth session, notifications, offline-sync queue) that many unrelated features need to read and that benefit from strict auditability. A single uniform tool for all of these either over-engineers trivial local state (full NgRx boilerplate for a dialog's open/closed flag) or under-engineers genuinely global, high-stakes state (informal signals for auth session with no time-travel debugging or action log).

# Selected variant

**Selected variant:** [[#Three-tier: Signals / NgRx Signal Store / classical NgRx Store]]

Match the tool to the scope of the state: Angular Signals for anything owned by a single component, NgRx Signal Store for anything owned by a single feature, and classical NgRx Store (actions/reducers/effects/selectors) for state that is genuinely global or cross-cutting — auth, notifications, and the offline-sync queue in particular, where an auditable action log and effect-based retry/conflict handling matter.

# Searched variants

## Three-tier: Signals / NgRx Signal Store / classical NgRx Store

### Description

- **Component-local state** (dialog visibility, selected tab, form draft, loading flags scoped to one component): plain Angular Signals declared directly in the component, no store of any kind.
- **Feature-level state** (the data and UI state owned by one feature, e.g. the list of orders being managed): an NgRx Signal Store colocated in that feature's `feature` lib (see solution #1's `{feature}/feature` structure).
- **Global/cross-cutting state** (auth session, notifications, offline-sync queue, and any other state read by multiple unrelated features): classical NgRx Store with actions/reducers/effects/selectors, in a shared `libs/shared/state` lib.

### Benefits

- Each kind of state gets a tool whose cost matches its actual complexity — no NgRx boilerplate for a dialog flag, no under-engineered ad hoc signals for auth
- Feature-level Signal Stores stay colocated with the feature that owns them, consistent with solution #1's module-boundary rules
- Global state keeps NgRx's action log, time-travel debugging, and effect-based retry/conflict handling exactly where it earns its cost: auth flows and offline-sync, which the planned offline-first solution will build on directly
- New engineers have one clear rule ("what scope is this state?") instead of a single tool stretched to fit every case

### Costs

- Three concepts to teach instead of one; a new engineer must learn where the boundary between "feature-level" and "global" sits
- Migrating a piece of state from one tier to another (e.g. a feature-level concern turning out to be needed globally) requires a deliberate refactor, not just a config change

## Classical NgRx Store for everything

### Description

Use actions/reducers/effects/selectors uniformly for all state, including component-local UI state.

### Benefits

- One mental model and one set of devtools for all state in the application
- Maximum auditability (every state change is an action) everywhere, not just where it matters most

### Costs

- Significant boilerplate for trivial, purely local state (a dialog's open/closed flag does not need an action/reducer/selector triad)
- Slower to write and review for features with a lot of small, ephemeral UI state

## NgRx Signal Store for everything

### Description

Use Signal Store uniformly, including for state that this solution classifies as global/cross-cutting (auth, notifications, offline-sync).

### Benefits

- Single, simpler mental model without actions/reducers/effects boilerplate
- Less code for the common case of feature-level state

### Costs

- Auth, notifications, and offline-sync benefit specifically from NgRx's action log and effect-based retry/conflict-handling patterns, which Signal Store does not provide in the same explicit, auditable form
- Loses the clear separation between "this is genuinely global and cross-cutting" and "this is feature-scoped," which matters once several unrelated features need to read the same global state

## Angular Signals for everything (no NgRx at all)

### Description

Use plain Signals and services for all state, including global concerns, with no NgRx dependency at all.

### Benefits

- Smallest possible dependency footprint and learning curve
- No NgRx-specific mental model to teach at all

### Costs

- Global, cross-cutting state (auth, notifications, offline-sync) loses auditability, a structured action log, and effect-based retry/conflict handling that the planned offline-sync solution specifically needs
- Ad hoc service-based patterns tend to reinvent, inconsistently, what NgRx already standardizes for exactly this kind of state
