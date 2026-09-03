---
name: plateau-offline-read-monolith--class-component-name-component-spec
description: Generic pattern for a component test under spec/, using Angular Testing Library and interacting through the rendered DOM — a component is tested purely on its own input()/output()/model() surface, independent of whatever business logic (if any) sits around it — offline-read-monolith plateau
domain: skill
type: template
whenToUse: when writing or reviewing a component's behavioural spec (Testing Library) under spec/
plateau: offline-read-monolith
artifact_type: spec
version: 20260903090000
tags:
  - skill/template/class
  - plateau/offline-read-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
---

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|{component-name}.component.spec.ts.create]]


# How this generic file is used
Created at `spec/{component-name}.component.spec.ts` next to the component implementation. This is not tied to one concrete component, and applies identically to both plateaus this solution covers: a platform feature component (may fake a Signal Store, if it injects one) and a design-system component (pure `input()`/`output()`/`model()`, no store, no injected dependency to fake at all). In both cases the [behavioral component test](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/behavioral-component-testing.md) never mocks HTTP, never fakes a Facade/Client — see [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill|solution-app-testing]] for that separate, business-layer concern.

# Goals

- Verify what a user actually experiences (rendered DOM, interactions) rather than internal implementation details
- Test a component purely on its own input/output surface — it does not matter, and the test must not need to know, what (if anything) sits around the component in a real application

# Implementation changes

```typescript
// Platform plateau example — OrdersListComponent, faking its Signal Store
// File: libs/{feature}/feature/src/lib/{feature}/orders-list/spec/orders-list.component.spec.ts
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { OrdersListComponent } from '../orders-list.component';

describe('OrdersListComponent', () => {
  describe('rendering', () => {
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
  });

  describe('user interactions', () => {
    it('calls the store when the user clicks "Add order"', async () => {
      const storeMock = { orders: signal([]), loading: signal(false), addOrder: vi.fn() };
      await render(OrdersListComponent, {
        providers: [{ provide: OrdersStore, useValue: storeMock }],
      });

      await userEvent.click(screen.getByRole('button', { name: /add order/i }));
      expect(storeMock.addOrder).toHaveBeenCalled();
    });
  });
});
```

```typescript
// Design-system plateau example — DsButtonComponent, pure input()/output(), nothing to fake
// File: projects/design-system/src/lib/ds-button/spec/ds-button.component.spec.ts
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { DsButtonComponent } from '../ds-button.component';

describe('DsButtonComponent', () => {
  describe('rendering', () => {
    it('renders its label and reflects the disabled input', async () => {
      await render(DsButtonComponent, { inputs: { label: 'Save', disabled: true } });

      expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
    });
  });

  describe('interactions', () => {
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
});
```

# Rule changes

## MUST
- The file must be created at `spec/{component-name}.component.spec.ts` so all test files live under `spec/` and do not clutter the component directory root.
- Component tests must query and interact with the rendered DOM via Testing Library (`screen.getByRole`, `userEvent`), not via `fixture.componentInstance` or `fixture.debugElement` reaching into internals.
- A component test must never use `HttpTestingController`, let a real HTTP call occur, or fake a Facade/Client — if the component injects a Signal Store or Facade directly, the test fakes only that one dependency, going no further down than the component's own immediate collaborator.
- A component that injects no dependency at all (e.g. a pure `input()`/`output()`/`model()` design-system component) must have a test that provides nothing beyond its own inputs — no provider setup is needed or expected.
- Queries must prefer accessible roles/labels (`getByRole`, `getByLabelText`) over test-id attributes, so the test also implicitly checks the component is accessible.
- Component tests must group related cases under nested `describe('<behavior-area>', () => { ... })` blocks (e.g., `rendering`, `user interactions`).

## SHOULD
- **Asserting against `fixture.componentInstance.someSignal()` instead of the rendered DOM** — Consequence: couples the test to the component's internal implementation — a refactor that preserves user-visible behavior but renames an internal signal breaks the test for no real reason — Instead: assert what `screen` shows, exactly as a user would perceive it
- **Reaching for `getByTestId` as the default query** — Consequence: misses the chance to verify the component is actually accessible (has correct roles/labels), and produces brittle selectors tied to markup rather than meaning — Instead: prefer `getByRole`/`getByLabelText`; reserve `getByTestId` for elements with no meaningful accessible role
- **Wiring up a Facade/Client mock "just in case" for a component that doesn't inject one** — Consequence: invents a dependency the component doesn't actually have, adding noise and a false impression that the component is more coupled than it is — Instead: only fake what the component actually injects — often nothing at all

# Check list

- [ ] The file is created at `spec/{component-name}.component.spec.ts`, not next to `component.ts`
- [ ] No test reaches into `fixture.componentInstance` or `debugElement` for assertions
- [ ] Every test fakes only what the component itself directly injects (a Signal Store/Facade, or nothing), never performs a real HTTP call
- [ ] Queries prefer accessible roles/labels over test IDs
- [ ] Related test cases are grouped under nested `describe('<behavior-area>', ...)` blocks

# Unittest TestCases

- [ ] WHEN the store's `loading` signal is true THEN
  - [ ] the rendered DOM shows a loading indicator
- [ ] WHEN the user interacts with a control that triggers a store method THEN
  - [ ] the faked store method is called with the expected arguments
- [ ] WHEN a pure input/output component's input changes THEN
  - [ ] the rendered DOM reflects the new input value, with no provider/mock setup involved
