---
name: forms-approach
description: Choice of form-building approach for new forms across the application
problem: Whether new forms should be built with Signal Forms or Reactive Forms, given that Signal Forms graduated to stable in Angular 22 and the rest of the architecture (state management) already standardizes on Signals
decision: Use Signal Forms as the default for all new forms; Reactive Forms is permitted only where an existing form is not being touched, or where a specific, documented limitation of Signal Forms in the current Angular version makes it impractical
tags:
  - solution/forms
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Angular now ships two supported ways to build forms: the long-standing Reactive Forms (`FormGroup`/`FormControl`, RxJS-based) and Signal Forms (`form()`/`FieldTree`, Signal-based), which graduated from experimental to stable in Angular 22. The "State management" solution already standardizes the rest of the application's reactive state on Signals (component-local Signals, NgRx Signal Store for features). We need a single default for how new forms are built, and a clear rule for what happens to forms already written before this solution was adopted.

# Selected variant

**Selected variant:** [[#Signal Forms as the default for all new forms]]

Signal Forms is adopted as the default for every new form, following Angular's own guidance that Signal Forms work best in new, Signals-based applications — which describes this codebase given the "State management" solution. Reactive Forms is not deprecated and remains fully supported, so existing forms are not forced to migrate; it stays available for the specific, documented cases where it is genuinely the better fit (see Rules in the implementation files for what qualifies).

# Searched variants

## Signal Forms as the default for all new forms

### Description

Every new form is built with `form()`/`FieldTree` from `@angular/forms/signals`. Validation uses the built-in validators (`required`, `minDate`, `maxDate`, `validateHttp()`, etc.) and the `when` option for conditional validation. Submission goes through `submitForm()`'s options-based API. Existing Reactive Forms are left as-is unless a form is being substantially rewritten anyway.

### Benefits

- Consistent reactivity model across the whole application: component-local Signals, NgRx Signal Store, and now forms all use the same Signal-based mental model — no RxJS subscription management mixed into form code
- Matches Angular's own current guidance: Signal Forms work best in new applications built with signals, which is exactly this architecture's direction
- Less boilerplate for the common case — no manually constructed `FormGroup`/`FormControl` tree mirroring the data model; `form()` takes the data Signal directly
- Fine-grained reactivity: only the template expressions reading a specific field's Signal re-render when that field changes, which matters for large, complex forms
- `ControlValueAccessor` components (relevant for design-system-provided custom controls) are compatible with Signal Forms' `formField` binding, so the design system does not need two parallel integration paths

### Costs

- Requires the workspace to run Angular 22 or newer (the current workspace, per solution #1, is on Angular 21 at the time this ADR is written — an upgrade is a prerequisite)
- Team must learn a new mental model (`FieldTree`, `form()`) distinct from the `FormGroup`/`FormControl` tree many engineers already know
- Fewer years of production battle-testing than Reactive Forms, even though it is now officially stable
- Large, heavily customized forms with many third-party `ControlValueAccessor` components may take more effort to migrate if ever ported from existing Reactive Forms code — though this cost only applies to migration, not to new forms

## Reactive Forms as the default, Signal Forms adopted later

### Description

Keep Reactive Forms as the standard for all new forms for now; revisit Signal Forms adoption in a future ADR once it has more production track record.

### Benefits

- Maximum production track record — Reactive Forms is stable, well-tested in production across millions of Angular applications, with no deprecation notice
- No need to upgrade the workspace's Angular version specifically to unlock this solution
- Every engineer already familiar with Angular knows this API

### Costs

- Leaves forms as the one major piece of the architecture still built on RxJS-based, imperative patterns (manual `valueChanges` subscriptions, manual cleanup) while state management (solution #3) has already moved to Signals — an inconsistency in the overall reactive model
- Delays capturing the boilerplate and fine-grained-reactivity benefits Signal Forms already provides today, for no concrete blocking reason beyond caution
- "Revisit later" decisions without a trigger condition tend to persist indefinitely

## Hybrid: Signal Forms for simple forms, Reactive Forms for complex forms with many custom controls

### Description

Pick per-form based on complexity: straightforward forms use Signal Forms; forms with many design-system `ControlValueAccessor` components or complex cross-field logic stay on Reactive Forms.

### Benefits

- Avoids any risk on the hardest, highest-stakes forms by keeping them on the most battle-tested API
- Lets the team gain Signal Forms experience gradually on lower-risk forms first

### Costs

- Two parallel form-building patterns to teach and maintain indefinitely, with a judgment call required for every new form about which one to use
- Angular 22 explicitly makes `ControlValueAccessor` components compatible with `formField`, which was the main technical reason to keep complex, control-heavy forms on Reactive Forms — that reason is largely resolved as of Angular 22
- Cross-field dependencies are precisely the case where Signal Forms benefits the most, per Angular's own guidance — routing exactly this kind of form to Reactive Forms forgoes the biggest win of the new API
