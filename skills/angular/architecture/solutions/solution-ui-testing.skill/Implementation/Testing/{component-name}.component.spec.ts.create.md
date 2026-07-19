---
description: Generic pattern for component tests using Angular Testing Library, interacting through the rendered DOM — a component is tested purely on its own input()/output()/model() surface, independent of whatever business logic (if any) sits around it
project_name: "{Feature}"
name: "{component-name}"
element_kind: component
change_kind: create
---

# How this generic file is used
This is not tied to one concrete component, and applies identically to both plateaus this solution covers: a platform feature component (may fake a Signal Store, if it injects one) and a design-system component (pure `input()`/`output()`/`model()`, no store, no injected dependency to fake at all). In both cases the [behavioral component test](../../glossary/behavioral-component-testing.md) never mocks HTTP, never fakes a Facade/Client — see [[skills/angular/architecture/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]] for that separate, business-layer concern.

# Goals

- Verify what a user actually experiences (rendered DOM, interactions) rather than internal implementation details
- Test a component purely on its own input/output surface — it does not matter, and the test must not need to know, what (if anything) sits around the component in a real application

# Implementation changes

```typescript
// Platform plateau example — OrdersListComponent, faking its Signal Store
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

```typescript
// Design-system plateau example — DsButtonComponent, pure input()/output(), nothing to fake
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';

describe('DsButtonComponent', () => {
  it('renders its label and reflects the disabled input', async () => {
    await render(DsButtonComponent, { inputs: { label: 'Save', disabled: true } });

    expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
  });

  it('emits (pressed) when clicked', async () => {
    const pressed = vi.fn();
    await render(DsButtonComponent, {
      inputs: { label: 'Save' },
      on: { pressed },
    });

    await userEvent.click(screen.getByRole('button', { name: /save/i }));
    expect(pressed).toHaveBeenCalled();
  });
});
```

# Rule changes

## MUST
- Component tests MUST query and interact with the rendered DOM via Testing Library (`screen.getByRole`, `userEvent`), not via `fixture.componentInstance` or `fixture.debugElement` reaching into internals.
- A component test MUST NOT use `HttpTestingController`, let a real HTTP call occur, or fake a Facade/Client — if the component injects a Signal Store or Facade directly, the test fakes only that one dependency, going no further down than the component's own immediate collaborator.
- A component that injects no dependency at all (e.g. a pure `input()`/`output()`/`model()` design-system component) MUST have a test that provides nothing beyond its own inputs — no provider setup is needed or expected.
- Queries MUST prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes, so the test also implicitly checks the component is accessible.

# Anti-patterns

- **Asserting against `fixture.componentInstance.someSignal()` instead of the rendered DOM**
  - Consequence: couples the test to the component's internal implementation — a refactor that preserves user-visible behavior but renames an internal signal breaks the test for no real reason
  - Instead: assert what `screen` shows, exactly as a user would perceive it

- **Reaching for `getByTestId` as the default query**
  - Consequence: misses the chance to verify the component is actually accessible (has correct roles/labels), and produces brittle selectors tied to markup rather than meaning
  - Instead: prefer `getByRole`/`getByLabelText`; reserve `getByTestId` for elements with no meaningful accessible role

- **Wiring up a Facade/Client mock "just in case" for a component that doesn't inject one**
  - Consequence: invents a dependency the component doesn't actually have, adding noise and a false impression that the component is more coupled than it is
  - Instead: only fake what the component actually injects — often nothing at all

# Check list

- [ ] No test reaches into `fixture.componentInstance` or `debugElement` for assertions
- [ ] Every test fakes only what the component itself directly injects (a Signal Store/Facade, or nothing), never performs a real HTTP call
- [ ] Queries prefer accessible roles/labels over test IDs

# Unittest TestCases

- [ ] WHEN the store's `loading` signal is true THEN
  - [ ] the rendered DOM shows a loading indicator
- [ ] WHEN the user interacts with a control that triggers a store method THEN
  - [ ] the faked store method is called with the expected arguments
- [ ] WHEN a pure input/output component's input changes THEN
  - [ ] the rendered DOM reflects the new input value, with no provider/mock setup involved
