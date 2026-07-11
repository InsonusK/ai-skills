---
name: class-form-component
description: Generic pattern for a form component built with Signal Forms — applies to any component in any feature that renders a form, tested via Testing Library against the rendered DOM
domain: skill
type: template
plateau: platform-monolith
artifact_type: component
version: 20260711210000
tags:
  - skill/template/class
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Generic pattern, not tied to one concrete form — every component that renders a form inside `libs/{feature}/feature` follows this, substituting `{form-name}` with the real form's name.

# Goal

- Build forms as Signal Forms by default, consistent with the rest of the application's Signal-based reactivity
- Give components synchronous, fine-grained access to field-level validity/touched/error state without manual RxJS subscriptions
- Verify what a user actually experiences (rendered DOM, interactions) rather than internal implementation details

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Field schema/validators stay inline in the component for simple forms, and are extracted into a `{form-name}.form.ts` file once cross-field logic or validator count makes the component harder to read
- Form submission always goes through `submitForm()`, and any resulting HTTP call goes through the owning feature's data-access Facade, never directly through `HttpClient`
- The component's spec queries and interacts with the rendered DOM via Testing Library, faking the component's Signal Store — never a real HTTP call

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Form component | `{FormName}FormComponent` | `OrderFormComponent` | `{form-name}.component.ts` | `order-form.component.ts` |
| Extracted field schema (only when non-trivial) | `{formName}Form` | `orderForm` | `{form-name}.form.ts` | `order-form.form.ts` |
| Component spec | — | — | `{form-name}.component.spec.ts` | `order-form.component.spec.ts` |

# Implementation

Simple form — schema defined inline in the component:

```typescript
// Skill: class-form-component
// Plateau: platform-monolith
// Version: 20260711210000

@Component({ /* ... */ })
export class LoginFormComponent {
  protected readonly credentials = signal({ email: '', password: '' });
  protected readonly loginForm = form(this.credentials, (path) => {
    required(path.email);
    required(path.password);
  });

  async submit() {
    const ok = await submitForm(this.loginForm, async (value) => {
      return this.authFacade.login(value);
    });
    if (!ok) {
      // form's own field-level errors already reflect what went wrong
    }
  }
}
```

Component spec — Testing Library, faking the Signal Store:

```typescript
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';

describe('OrdersListComponent', () => {
  it('shows the loading state, then the list of orders', async () => {
    const storeMock = {
      orders: signal([{ id: '1', quantity: 2 }]),
      loading: signal(false),
      load: vi.fn(),
    };
    await render(OrdersListComponent, {
      providers: [{ provide: OrdersStore, useValue: storeMock }],
    });

    expect(screen.getByText(/quantity: 2/i)).toBeInTheDocument();
  });

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
- A form's submission MUST go through `submitForm()`, not a manually wired `(ngSubmit)` handler that bypasses the form's own validation/state.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade, never call `HttpClient` directly from the form component.
- A custom, design-system-provided form control used inside a Signal Form MUST implement `ControlValueAccessor` so it is compatible with `formField` binding.
- Component tests MUST query and interact with the rendered DOM via Testing Library (`screen.getByRole`, `userEvent`), not via `fixture.componentInstance` or `fixture.debugElement` reaching into internals.
- A component test MUST fake the component's Signal Store (or Facade, if the component calls one directly) — it MUST NOT use `HttpTestingController` or let a real HTTP call occur.

## SHOULD
- Field schema/validators SHOULD stay inline in the component for simple forms, and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation (`when`) or the number of validators makes the component harder to read.
- Async validation SHOULD use `validateHttp()` with its built-in debounce, rather than a hand-rolled debounced subscription.
- Queries SHOULD prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes, so the test also implicitly checks the component is accessible.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Manually subscribing to a signal-based field's changes to imperatively trigger side effects**
  - Consequence: reintroduces the subscription-management problem Signal Forms exists to avoid
  - Instead: read the field's Signal directly in a computed value or an effect scoped to the component's lifecycle
- **Wiring a raw `HttpClient` call directly inside a form component's submit handler**
  - Consequence: bypasses the feature's data-access Facade
  - Instead: call through the feature's Facade from inside `submitForm()`'s callback
- **Asserting against `fixture.componentInstance.someSignal()` instead of the rendered DOM**
  - Consequence: couples the test to the component's internal implementation — a refactor that preserves user-visible behavior but renames an internal signal breaks the test for no real reason
  - Instead: assert what `screen` shows, exactly as a user would perceive it
- **Reaching for `getByTestId` as the default query**
  - Consequence: misses the chance to verify the component is actually accessible, produces brittle markup-tied selectors
  - Instead: prefer `getByRole`/`getByLabelText`; reserve `getByTestId` for elements with no meaningful accessible role

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Check list

- [ ] The form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Submission goes through `submitForm()`
- [ ] Non-trivial field schemas are extracted into their own `.form.ts` file
- [ ] Any custom design-system control used in the form implements `ControlValueAccessor`
- [ ] No test reaches into `fixture.componentInstance` or `debugElement` for assertions
- [ ] Every test fakes the component's Signal Store/Facade, never performs a real HTTP call
- [ ] Queries prefer accessible roles/labels over test IDs

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a required field is left empty and the form is submitted THEN
  - [ ] `submitForm()` returns `false`
  - [ ] the field's `invalid()`/`errors()` signals reflect the validation failure
- [ ] WHEN all fields are valid and the form is submitted THEN
  - [ ] `submitForm()` calls the provided callback with the current form value
  - [ ] `submitForm()` returns `true` on a successful callback
- [ ] WHEN the store's `loading` signal is true THEN
  - [ ] the rendered DOM shows a loading indicator
- [ ] WHEN the user interacts with a control that triggers a store method THEN
  - [ ] the faked store method is called with the expected arguments

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create|Testing/{component-name}.component.spec.ts.create]]
