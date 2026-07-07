---
description: Generic pattern for component tests using Angular Testing Library, interacting through the rendered DOM and faking the component's Signal Store
project_name: "{Feature}"
name: "{component-name}"
artifact_type: component
change_kind: create
---

# How this generic file is used
This is not tied to one concrete component. Any component rendering a template gets a spec following this pattern, substituting the real feature/component names.

# Goals

- Verify what a user actually experiences (rendered DOM, interactions) rather than internal implementation details

# Implementation changes

```code example
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

# Rule changes

## MUST
- Component tests MUST query and interact with the rendered DOM via Testing Library (`screen.getByRole`, `userEvent`), not via `fixture.componentInstance` or `fixture.debugElement` reaching into internals.
- A component test MUST fake the component's Signal Store (or Facade, if the component calls one directly) — it MUST NOT use `HttpTestingController` or let a real HTTP call occur.
- Queries MUST prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes, so the test also implicitly checks the component is accessible.

# Anti-patterns

- **Asserting against `fixture.componentInstance.someSignal()` instead of the rendered DOM**
  - Consequence: couples the test to the component's internal implementation — a refactor that preserves user-visible behavior but renames an internal signal breaks the test for no real reason
  - Instead: assert what `screen` shows, exactly as a user would perceive it

- **Reaching for `getByTestId` as the default query**
  - Consequence: misses the chance to verify the component is actually accessible (has correct roles/labels), and produces brittle selectors tied to markup rather than meaning
  - Instead: prefer `getByRole`/`getByLabelText`; reserve `getByTestId` for elements with no meaningful accessible role

# Check list

- [ ] No test reaches into `fixture.componentInstance` or `debugElement` for assertions
- [ ] Every test fakes the component's Signal Store/Facade, never performs a real HTTP call
- [ ] Queries prefer accessible roles/labels over test IDs

# Unittest TestCases

- [ ] WHEN the store's `loading` signal is true THEN
  - [ ] the rendered DOM shows a loading indicator
- [ ] WHEN the user interacts with a control that triggers a store method THEN
  - [ ] the faked store method is called with the expected arguments
