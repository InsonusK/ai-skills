---
description: Extend the design system repository with the ds-* selector convention, signal-based API authoring, ControlValueAccessor requirement for form controls, and the per-component internal-implementation decision rule
element_kind: repository
change_kind: extend
tags:
  - solution/design-system-components
  - element/design-system-repository
---

# Structure

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| /projects/design-system/src/lib/{component-name} | One directory per component, following the generic pattern in [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create]]. |

# Rules

## MUST
- Every component uses the `ds-` selector prefix — never expose or re-export an Angular Material selector directly.
  - Risk: a re-exported `mat-*` selector lets consumers bind to Material's API and couples them to its versioning.
  - Fix: a `ds-`-prefixed selector on every component; Material stays behind it.
- Every component's public API uses `input()`, `output()`, `model()` — no `@Input()`/`@Output()` decorators, no `EventEmitter`.
  - Risk: mixed authoring styles across the library make the API inconsistent and block signal-based interop.
  - Fix: signal APIs only; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/adr/component-api-authoring-style.md|component-api-authoring-style]].
- Every component's API is designed around this application's usage axes — never mirrors Material's input names or category model 1:1.
  - Risk: a 1:1 mirror re-exposes Material's model and its churn, defeating the encapsulation this layer exists for.
  - Fix: name inputs for the app's concepts; map to Material internally; per [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/adr/component-encapsulation-strategy.md|component-encapsulation-strategy]].
- Any component that accepts user input as part of a form implements `ControlValueAccessor`.
  - Risk: without CVA the component cannot bind to Signal Forms `formField` and consumers wire it up ad hoc.
  - Fix: implement `ControlValueAccessor`; verify a `formField` binding works.
- No Angular Material selector, input, or type appears in this library's public API surface — check the built `dist/**/types/*.d.ts`, not just source.
  - Risk: a `protected`/`public` field typed as a Material type (e.g. `MatButtonAppearance`) leaks into the emitted `.d.ts` even when source "looks" encapsulated.
  - Fix: local literal types for such fields (`type MatAppearance = 'filled' | 'outlined' | 'text'`); grep the packed `types/*.d.ts` for `@angular/material`.

## SHOULD
- Before building a component's internal implementation, should first evaluate whether Angular Material's own equivalent component satisfies the real functional, performance, and accessibility requirements; delegate to it internally if so, and build a fully custom implementation only when it does not.

- **Exposing a Material input type or enum directly through a design system component's public API** — Consequence: leaks Material's own API surface to consumers, defeating the encapsulation this solution exists to provide, and coupling consumers to Material's own versioning — Instead: define the design system's own type/enum for that concept, mapped internally to whatever Material (or custom implementation) actually needs
- **Building a fully custom internal implementation by default, without first checking whether Material's own component already satisfies the requirement** — Consequence: unnecessary duplicated effort and maintenance burden for a component Material could have handled adequately — Instead: default to delegating to Material internally; only go custom when a real, identified gap (performance, missing feature, accessibility) justifies it
# Unittest TestCases

- [ ] WHEN a component's public API (its exported class, inputs, outputs) is inspected THEN
  - [ ] no Angular Material type, selector, or enum appears in it
- [ ] WHEN any component's source is inspected THEN
  - [ ] no `@Input()`/`@Output()` decorator or `EventEmitter` is used
