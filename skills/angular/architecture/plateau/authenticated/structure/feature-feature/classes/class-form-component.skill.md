---
name: class-form-component
description: Generic pattern for a form component built with Signal Forms — applies to any component in any feature that renders a form
domain: skill
type: template
plateau: authenticated
artifact_type: component
version: 20260711150000
tags:
  - skill/template/class
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]]"
---

> Generic pattern, not tied to one concrete form — every component that renders a form inside `libs/{feature}/feature` follows this, substituting `{form-name}` with the real form's name.

# Goal

- Build forms as Signal Forms by default, consistent with the rest of the application's Signal-based reactivity
- Give components synchronous, fine-grained access to field-level validity/touched/error state without manual RxJS subscriptions

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Field schema/validators stay inline in the component for simple forms, and are extracted into a `{form-name}.form.ts` file once cross-field logic or validator count makes the component harder to read
- Form submission always goes through `submitForm()`, and any resulting HTTP call goes through the owning feature's data-access Facade, never directly through `HttpClient`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Form component | `{FormName}FormComponent` | `OrderFormComponent` | `{form-name}.component.ts` | `order-form.component.ts` |
| Extracted field schema (only when non-trivial) | `{formName}Form` | `orderForm` | `{form-name}.form.ts` | `order-form.form.ts` |

# Implementation

Simple form — schema defined inline in the component:

```typescript
// Skill: class-form-component
// Plateau: data-capable
// Version: 20260711150000

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

Non-trivial form — field schema extracted into its own file once validators/cross-field logic grow large enough to obscure the component:

```typescript
// order-form.form.ts
export function createOrderForm(data: Signal<OrderFormValue>) {
  return form(data, (path) => {
    required(path.customerName);
    minDate(path.deliveryDate, () => new Date());
    when(path.isGift, () => required(path.giftMessage));
  });
}

// order-form.component.ts
export class OrderFormComponent {
  protected readonly orderData = signal<OrderFormValue>(initialOrderFormValue);
  protected readonly orderForm = createOrderForm(this.orderData);

  async submit() {
    const ok = await submitForm(this.orderForm, async (value) => {
      return this.ordersStore.addOrder(value);
    });
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Rules

## MUST
- A form's submission MUST go through `submitForm()`, not a manually wired `(ngSubmit)` handler that bypasses the form's own validation/state.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` Facade (typically via the feature's Signal Store), never call `HttpClient` directly from the form component.
- A custom, design-system-provided form control used inside a Signal Form MUST implement `ControlValueAccessor` so it is compatible with `formField` binding.

## SHOULD
- Field schema/validators SHOULD stay inline in the component for simple forms (a handful of fields, no cross-field logic), and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation (`when`) or the number of validators makes the component harder to read.
- Async validation (e.g. "is this email already taken") SHOULD use `validateHttp()` with its built-in debounce, rather than a hand-rolled debounced subscription.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Manually subscribing to a signal-based field's changes to imperatively trigger side effects**
  - Consequence: reintroduces the subscription-management problem Signal Forms exists to avoid, and can desynchronize from the form's own reactivity
  - Instead: read the field's Signal directly in a computed value or an effect scoped to the component's lifecycle
- **Wiring a raw `HttpClient` call directly inside a form component's submit handler**
  - Consequence: bypasses the feature's data-access Facade, duplicating error handling and mapping logic that belongs in one place
  - Instead: call through the feature's Facade from inside `submitForm()`'s callback

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Check list

- [ ] The form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Submission goes through `submitForm()`
- [ ] Non-trivial field schemas (cross-field `when` logic, many validators) are extracted into their own `.form.ts` file
- [ ] Any custom design-system control used in the form implements `ControlValueAccessor`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]

# Unittest TestCases

- [ ] WHEN a required field is left empty and the form is submitted THEN
  - [ ] `submitForm()` returns `false`
  - [ ] the field's `invalid()`/`errors()` signals reflect the validation failure
- [ ] WHEN all fields are valid and the form is submitted THEN
  - [ ] `submitForm()` calls the provided callback with the current form value
  - [ ] `submitForm()` returns `true` on a successful callback
- [ ] WHEN a conditional (`when`) validator's condition becomes true THEN
  - [ ] the dependent field becomes invalid if it does not satisfy the now-active validator

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-forms.skill/solution-forms.skill|solution-forms]] - [[skills/angular/architecture/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend|FormComponent/{component-name}.component.ts.extend]]
