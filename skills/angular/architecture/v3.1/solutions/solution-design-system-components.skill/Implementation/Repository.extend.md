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
- Every component must use the `ds-` selector prefix — never expose or re-export an Angular Material selector directly.
- Every component's public API must use `input()`, `output()`, and `model()` — no `@Input()`/`@Output()` decorators, no `EventEmitter`, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/adr/component-api-authoring-style.md|component-api-authoring-style]].
- Every component's API must be designed around this application's actual usage axes — it must never mirror Angular Material's own input names or category model 1:1, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/adr/component-encapsulation-strategy.md|component-encapsulation-strategy]].
- Any component accepting user input as part of a form (per `solution-forms`'s Signal Forms integration) must implement `ControlValueAccessor`, so it is compatible with `formField` binding.
- No Angular Material selector, input, or type must appear in this library's public API surface (i.e. in what a consuming application imports or binds to) — Material may be used entirely internally.

## SHOULD
- Before building a component's internal implementation, should first evaluate whether Angular Material's own equivalent component satisfies the real functional, performance, and accessibility requirements; delegate to it internally if so, and build a fully custom implementation only when it does not.

- **Exposing a Material input type or enum directly through a design system component's public API** — Consequence: leaks Material's own API surface to consumers, defeating the encapsulation this solution exists to provide, and coupling consumers to Material's own versioning — Instead: define the design system's own type/enum for that concept, mapped internally to whatever Material (or custom implementation) actually needs
- **Building a fully custom internal implementation by default, without first checking whether Material's own component already satisfies the requirement** — Consequence: unnecessary duplicated effort and maintenance burden for a component Material could have handled adequately — Instead: default to delegating to Material internally; only go custom when a real, identified gap (performance, missing feature, accessibility) justifies it
# Unittest TestCases

- [ ] WHEN a component's public API (its exported class, inputs, outputs) is inspected THEN
  - [ ] no Angular Material type, selector, or enum appears in it
- [ ] WHEN any component's source is inspected THEN
  - [ ] no `@Input()`/`@Output()` decorator or `EventEmitter` is used
