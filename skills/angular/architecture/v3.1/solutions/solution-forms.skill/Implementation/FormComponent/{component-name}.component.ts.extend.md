---
description: Generic pattern for building a form component with Signal Forms — applies to any component in any feature that renders a form
project_name: "{Feature}"
name: "{form-name}"
element_kind: component
change_kind: extend
tags:
  - solution/forms
  - element/component-name-component-ts
---

# How this generic file is used
This is not tied to one concrete form. Any component that renders a form follows this pattern, substituting `{form-name}` with the real form's name.

# Goals

- Build forms as Signal Forms by default, consistent with the rest of the application's Signal-based reactivity (see the "State management" solution)
- Give components synchronous, fine-grained access to field-level validity/touched/error state without manual RxJS subscriptions

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------ | -------------------- | --------- |
| Form component | {FormName}FormComponent | OrderFormComponent | {form-name}.component.ts | order-form.component.ts |
| Extracted field schema (only when non-trivial) | {formName}Form | orderForm | {form-name}.form.ts | order-form.form.ts |

# Implementation changes

Simple form — schema defined inline in the component:

```typescript
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

Non-trivial form — field schema extracted into its own file when validators/cross-field logic grow large enough to obscure the component:

```typescript
// order-form.form.ts
export function createOrderForm(data: Signal<OrderFormValue>) {
  return form(data, (path) => {
    required(path.customerName);
    minDate(path.deliveryDate, () => new Date());
    when(path.isGift, () => required(path.giftMessage));
  });
}
```

```typescript
// order-form.component.ts
export class OrderFormComponent {
  protected readonly orderData = signal<OrderFormValue>(initialOrderFormValue);
  protected readonly orderForm = createOrderForm(this.orderData);
}
```

# Rule changes

## MUST
- A form's submission MUST go through `submitForm()`, not a manually wired `(ngSubmit)` handler that bypasses the form's own validation/state.
- Any HTTP call triggered by form submission MUST go through the owning feature's `data-access` facade (see the future `solution-api-http-layer`), never call `HttpClient` directly from the form component.
- A custom, design-system-provided form control used inside a Signal Form MUST implement `ControlValueAccessor` so it is compatible with `formField` binding (see `solution-design-system-application` for where this contract is defined).

## SHOULD
- Field schema/validators SHOULD stay inline in the component for simple forms (a handful of fields, no cross-field logic), and SHOULD be extracted into a `{form-name}.form.ts` file once cross-field validation (`when`) or the number of validators makes the component harder to read.
- Async validation (e.g. "is this email already taken") SHOULD use `validateHttp()` with its built-in debounce, rather than a hand-rolled debounced subscription.

- **Manually subscribing to a signal-based field's changes to imperatively trigger side effects** — Consequence: reintroduces the subscription-management problem Signal Forms exists to avoid, and can desynchronize from the form's own reactivity — Instead: read the field's Signal directly in a computed value or an effect scoped to the component's lifecycle
- **Wiring a raw `HttpClient` call directly inside a form component's submit handler** — Consequence: bypasses the feature's data-access facade, duplicating error handling and mapping logic that belongs in one place — Instead: call through the feature's facade from inside `submitForm()`'s callback
# Check list

- [ ] The form is built with `form()`/`FieldTree`, not `FormGroup`/`FormControl`
- [ ] Submission goes through `submitForm()`
- [ ] Non-trivial field schemas (cross-field `when` logic, many validators) are extracted into their own `.form.ts` file
- [ ] Any custom design-system control used in the form implements `ControlValueAccessor`

# Unittest TestCases

- [ ] WHEN a required field is left empty and the form is submitted THEN
  - [ ] `submitForm()` returns `false`
  - [ ] the field's `invalid()`/`errors()` signals reflect the validation failure
- [ ] WHEN all fields are valid and the form is submitted THEN
  - [ ] `submitForm()` calls the provided callback with the current form value
  - [ ] `submitForm()` returns `true` on a successful callback
- [ ] WHEN a conditional (`when`) validator's condition becomes true THEN
  - [ ] the dependent field becomes invalid if it does not satisfy the now-active validator
