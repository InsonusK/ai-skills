---
name: class-component-name-component
description: Generic pattern for any component owned by a feature — Signal Forms for forms, and Testing-Library-based component tests, applies to any component in any feature
domain: skill
type: template
plateau: offline-app
artifact_type: component
version: 20260711140000
tags:
  - skill/template/class
  - plateau/offline-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Generic pattern, not tied to one concrete component. Component-local state (dialog visibility, selected tab, form draft, loading flags) always uses a plain `signal()` — see [[../../repo-offline-app.skill.md#Cross-cutting conventions|repo-offline-app's cross-cutting conventions]] for that rule; it applies to every component in the workspace, not only this feature template.

# Goal

- Build forms as Signal Forms by default, giving components synchronous, fine-grained access to field-level validity/touched/error state
- Verify what a user actually experiences (rendered DOM, interactions) rather than internal implementation details

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- A form's submission goes through `submitForm()`, never a manually wired `(ngSubmit)` handler that bypasses the form's own validation
- Component tests query and interact with the rendered DOM via Testing Library, never `fixture.componentInstance`/`debugElement` internals

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Form component | `{FormName}FormComponent` | `OrderFormComponent` | `{form-name}.component.ts` | `order-form.component.ts` |
| Extracted field schema (only when non-trivial) | `{formName}Form` | `orderForm` | `{form-name}.form.ts` | `order-form.form.ts` |
| Component spec | `{ComponentName}Component` (tested) | `OrdersListComponent` | `{component-name}.component.spec.ts` | `orders-list.component.spec.ts` |

# Implementation

Simple form — schema defined inline in the component:

```typescript
// Skill: class-component-name-component
// Plateau: offline-app
// Version: 20260711140000

@Component({ /* ... */ })
export class LoginFormComponent {
  protected readonly credentials = signal({ email: '', password: '' });
  protected readonly loginForm = form(this.credentials, (path) => {
    required(path.email);
    required(path.password);
  });

  async submit() {
    await submitForm(this.loginForm, async (value) => this.authFacade.login(value));
  }
}
```

Component test — Testing Library, faking the Signal Store:

```typescript
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';

describe('OrdersListComponent', () => {
  it('calls the store when the user clicks "Add order"', async () => {
    const storeMock = { orders: signal([]), loading: signal(false), addOrder: vi.fn() };
    await render(OrdersListComponent, {
      providers: [{ provide: OrdersStore, useValue: storeMock }],
    });

    await userEvent.click(screen.getByRole('button', { name: /add order/i }));
    expect(storeMock.addOrder).toHaveBeenCalled();
  });
});
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Rules

## MUST
- A form's submission MUST go through `submitForm()`.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade, never call `HttpClient` directly from the form component.
- A custom design-system-provided form control used inside a Signal Form MUST implement `ControlValueAccessor`.
- Component tests MUST query and interact with the rendered DOM via Testing Library (`screen.getByRole`, `userEvent`).
- A component test MUST fake the component's Signal Store (or Facade), never use `HttpTestingController` or let a real HTTP call occur.

## SHOULD
- Field schema/validators SHOULD stay inline for simple forms, and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation (`when`) makes the component harder to read.
- Async validation SHOULD use `validateHttp()` with its built-in debounce.
- Queries SHOULD prefer accessible roles/labels over test-id attributes.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Manually subscribing to a signal-based field's changes to imperatively trigger side effects**
  - Consequence: reintroduces the subscription-management problem Signal Forms exists to avoid
  - Instead: read the field's Signal directly in a computed value or a scoped effect
- **Wiring a raw `HttpClient` call directly inside a form component's submit handler**
  - Consequence: bypasses the feature's data-access facade
  - Instead: call through the feature's facade from inside `submitForm()`'s callback
- **Asserting against `fixture.componentInstance.someSignal()` instead of the rendered DOM**
  - Consequence: couples the test to internal implementation
  - Instead: assert what `screen` shows, exactly as a user would perceive it

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Check list

- [ ] The form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Submission goes through `submitForm()`
- [ ] No test reaches into `fixture.componentInstance` or `debugElement` for assertions
- [ ] Every test fakes the component's Signal Store/Facade, never performs a real HTTP call

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a required field is left empty and the form is submitted THEN
  - [ ] `submitForm()` returns `false` and the field's `invalid()`/`errors()` signals reflect the failure
- [ ] WHEN all fields are valid and the form is submitted THEN
  - [ ] `submitForm()` calls the provided callback and returns `true`
- [ ] WHEN the store's `loading` signal is true THEN
  - [ ] the rendered DOM shows a loading indicator
- [ ] WHEN the user interacts with a control that triggers a store method THEN
  - [ ] the faked store method is called with the expected arguments

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]
