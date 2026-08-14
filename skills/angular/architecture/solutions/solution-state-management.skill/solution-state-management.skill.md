---
name: solution-state-management
description: Three-tier state management policy — Angular Signals for component-local state, NgRx Signal Store for feature-level state, classical NgRx Store for global/cross-cutting state
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - stack/typescript
  - ngrx
  - signals
  - state-management
  - framework/angular
  - concern/architecture
  - solution/state-management

triggers:
  - Deciding where a new piece of state should live
  - Adding state to a new or existing feature
  - Reviewing whether a component, feature, or global store owns a given piece of state correctly
creates:
  - libs/shared/state
extends:
  - libs/{feature}/feature (feature-level Signal Store)
  - "{component-name}.component.ts (component-local Signals)"
depends_on:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]"
adr:
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/adr/state-management-tiering|State Management Tiering ADR]]"
---

# Goal

- Give every piece of application state an unambiguous home based on its actual scope, instead of one tool stretched to fit every case
- Keep feature-level state colocated with its owning feature, consistent with the module boundaries from solution #1
- Give global/cross-cutting state (auth, notifications, offline-sync) the auditability and effect-based retry/conflict handling it needs, since the planned offline-sync solution builds directly on this

# Capabilities

- No NgRx boilerplate for purely local UI state
- Feature-level state stays encapsulated inside its owning feature's `feature` lib, with no cross-feature state leakage
- A single, auditable source of truth for auth/notifications/offline-sync, safe to build the future offline-sync solution on top of
- A clear, teachable rule for "which tier does this state belong to" instead of ad hoc decisions per feature

# Core Principles

- **Component-local state** (dialog visibility, selected tab, form draft, component-scoped loading flags) is a plain Angular Signal on the component — no store of any kind
- **Feature-level state** (data and UI state owned by one feature) is an NgRx Signal Store, colocated inside that feature's `libs/{feature}/feature` project
- **Global/cross-cutting state** (auth session, notifications, offline-sync queue — read or dispatched by more than one unrelated feature) is a classical NgRx Store slice inside `libs/shared/state`
- State is promoted from a lower tier to a higher one only when a second, unrelated consumer genuinely needs it — never preemptively
- A feature store or component never duplicates global state locally; it always reads global state through `libs/shared/state` selectors

# Adr

- [[skills/angular/architecture/solutions/solution-state-management.skill/adr/state-management-tiering|Three-tier state management: Signals / NgRx Signal Store / classical NgRx Store]]
  - Selected variant: three-tier — chosen to give every piece of state an unambiguous home, with the global tier providing the auditability and effect-based retry/conflict handling specifically for auth and offline-sync

# Requirements

SOLUTION:
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|Структура репозитория (база)]]
  - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|libs/{feature}/feature]] - the feature-level Signal Store is colocated inside this project
  - `libs/shared` - hosts the new global/cross-cutting state slices (`libs/shared/state`)

# Template Skill Mutations

REPOSITORY:
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|Repository]] - extend - add `libs/shared/state`, the `type:store` tag, and the module-boundary rules for global state

PROJECT:
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create|libs/shared/state]] - create - host classical NgRx slices for auth, notifications, offline-sync
  - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|auth.store.ts]] - create - auth session slice (actions/reducer/effects/selectors)
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|{Feature}/feature (generic pattern)]] - extend - add a feature-level Signal Store
  - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|{feature}.store.ts]] - create - feature-level Signal Store pattern, applied by any future feature-owning solution
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|{component-name} (generic pattern)]] - extend - component-local state via plain Signals, applied to any component in the application

`auth.store.ts` is the concrete worked example. The `notifications` and `offline-sync` slices follow the same structural pattern (see [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create]]) and will be filled in by the solutions that actually introduce them.

# Workflow

## Deciding where new state belongs (happy path)

1. Is this state owned by exactly one component (and optionally its direct children)? → plain Signal on the component.
2. Otherwise, ask: is this state owned by exactly one feature? → NgRx Signal Store inside that feature's `feature` lib.
3. Otherwise (more than one unrelated feature needs to read or dispatch it) → classical NgRx Store slice inside `libs/shared/state`.

```mermaid
sequenceDiagram
    autonumber
    actor Dev
    participant Rule as Tiering rule
    Dev->>Rule: where does this state belong?
    activate Rule
    Rule-->>Dev: single component only? -> Signal
    Rule-->>Dev: single feature only? -> Feature Signal Store
    Rule-->>Dev: multiple unrelated features? -> libs/shared/state (classical NgRx)
    deactivate Rule
```

## Feature needs a cross-cutting value (happy path)

1. A feature Signal Store needs the current user (auth) to decide what to render.
2. Instead of duplicating a "current user" field locally, the feature store injects and reads `selectCurrentUser` from `libs/shared/state`'s auth slice.
3. If the session expires, the auth slice's state changes once, and every feature reading the shared selector reflects it — no manual synchronization needed.

## State misplacement (failure path / anti-pattern)

1. A feature store starts caching a copy of the current user locally "for convenience."
2. On logout, the global auth slice clears its state, but the feature's local copy does not update automatically.
3. The feature renders as if the user is still logged in — a divergence bug caused by skipping the tiering rule.
4. Fix: remove the local copy, read `selectCurrentUser` from `libs/shared/state` directly wherever it's needed.

# Rules

## MUST
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend#MUST|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create#MUST|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create#MUST|auth.store.ts]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create#MUST|{feature}.store.ts]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend#MUST|{component-name}.component.ts]]

## SHOULD
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create#SHOULD|{feature}.store.ts]]

## MUST NOT
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend#MUST NOT|Repository.extend]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create#MUST NOT|GlobalStore/shared-state.project.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create#MUST NOT|auth.store.ts]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend#MUST NOT|{component-name}.component.ts]]

# Anti-patterns

- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend|See Repository.extend.md]] — a feature caching auth state locally instead of selecting from `libs/shared/state`.
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/GlobalStore/shared-state.project.create/auth.store.ts.create|See auth.store.ts.create.md]] — dispatching HTTP calls directly from a component against this store's actions, bypassing effects.
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend/{feature}.store.ts.create|See {feature}.store.ts.create.md]] — a feature store re-implementing cross-cutting state instead of reading `libs/shared/state`.
- [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/LocalState/{component-name}.component.ts.extend|See {component-name}.component.ts.extend.md]] — creating a feature or global store for state only one component ever reads.

# Check list

- [ ] No component-local Signal has been unnecessarily promoted to a feature or global store
- [ ] `libs/shared/state` contains only genuinely global/cross-cutting slices (auth, notifications, offline-sync, and similar)
- [ ] Every feature-level Signal Store lives inside that feature's own `libs/{feature}/feature` project, not in a shared location
- [ ] `@nx/enforce-module-boundaries` passes with the extended allow-list from [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/Repository.extend]]