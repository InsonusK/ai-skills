---
name: component-api-authoring-style
description: How component inputs, outputs, and two-way bindings are declared
problem: Angular now offers a signal-based, function-based API (input(), output(), model()) as a full replacement for the decorator-based @Input()/@Output() + EventEmitter approach; the design system should commit to one consistent style rather than mixing both
decision: Use signal-based input(), output(), and model() exclusively — no @Input()/@Output() decorators, no EventEmitter
tags:
  - solution/design-system-components
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Angular's component-authoring API has evolved from decorator-based `@Input()`/`@Output()` (with `EventEmitter`) to a function-based, signal-aligned API: `input()` (stable since Angular 17.1), `output()` (introduced in Angular 17.3, stable in the versions this workspace targets), and `model()` for two-way binding. Both styles remain supported — Angular has not deprecated the decorators — but the whole platform (per the "State management" and `solution-forms`s) is already built around Signals as the primary reactivity primitive. The design system needs one consistent authoring convention, not a mix of both styles across components.

# Selected variant

**Selected variant:** [[#Signal-based input()/output()/model() exclusively]]

Every design system component declares its public API using `input()`, `output()`, and `model()` — never `@Input()`, `@Output()`, or `EventEmitter`.

# Searched variants

## Signal-based input()/output()/model() exclusively

### Description

`input()` exposes a bound value as a read-only `Signal`; `output()` provides a lightweight, RxJS-independent way to emit events (`OutputEmitterRef`, no `EventEmitter`); `model()` provides two-way binding without manually wiring a paired input/output.

### Benefits

- Consistent with this platform's overall commitment to Signals as the primary reactivity primitive (state management, forms) — the design system's own components follow the same model application code already uses
- `output()` removes the RxJS/`EventEmitter` dependency for basic eventing, producing lighter components
- `input()`'s value is a `Signal`, directly composable with `computed()`/`effect()` inside the component, consistent with how the rest of the platform already reasons about derived state
- A single, consistent authoring style across every component in the library — no per-component judgment call about which style to use

### Costs

- Contributors only familiar with the decorator-based style need to learn the function-based API — a modest, one-time learning cost
- `model()`/`output()` are comparatively newer APIs with a shorter track record than the long-standing decorator-based approach, though both are stable in the Angular version range this workspace targets

## @Input()/@Output() decorators with EventEmitter

### Description

The traditional, long-standing component-authoring style.

### Benefits

- Longest track record and widest historical familiarity
- No migration needed for any code already written this way

### Costs

- Inconsistent with the platform's broader commitment to Signals — the design system's own components would use a different reactivity model than the application code consuming them
- `EventEmitter`'s RxJS-based typing has a long-standing type-safety gap (it can accept values that shouldn't be emittable), which `output()` was specifically introduced to fix
- Mixing this style with the rest of a Signals-oriented codebase reads as legacy rather than as a deliberate current choice
