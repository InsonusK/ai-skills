---
name: plateau-design-system--class-component-name-component-spec
description: Generic behavioural spec for a ds-* component — Angular Testing Library through the rendered DOM, at spec/{component}.component.spec.ts, providing nothing beyond the component's own inputs — design-system plateau
domain: skill
type: template
whenToUse: when writing or reviewing a ds-* component's spec/{component}.component.spec.ts
plateau: design-system
artifact_type: component
version: 20260903170000
tags:
  - skill/template/class
  - plateau/design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
---

> `projects/design-system/src/lib/{component}/spec/{component}.component.spec.ts`. Runs under `ng test design-system` (Angular 22's `@angular/build:unit-test` builder — Vitest + jsdom). The method and rules are `solution-ui-testing`'s.

# Goal

- Verify what a user experiences (rendered DOM, interactions) on the component's own `input()`/`output()`/`model()` surface — a pure `ds-*` component injects nothing, so the test fakes nothing

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

# Implementation

```typescript
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { DsButtonComponent } from '../button.component';

describe('DsButtonComponent', () => {
  describe('rendering', () => {
    it('renders the label and reflects the disabled input', async () => {
      await render(DsButtonComponent, { inputs: { label: 'Save', disabled: true } });
      expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
    });
  });

  describe('interactions', () => {
    it('emits (pressed) when clicked', async () => {
      const pressed = vi.fn();
      await render(DsButtonComponent, { inputs: { label: 'Save' }, on: { pressed } });
      await userEvent.click(screen.getByRole('button', { name: /save/i }));
      expect(pressed).toHaveBeenCalledTimes(1);
    });
  });
});
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

# Rules

## MUST
- The file is `spec/{component-name}.component.spec.ts` — all test files under `spec/`.
- Query and interact via Testing Library (`screen.getByRole`, `userEvent`), never `fixture.componentInstance` / `debugElement`.
- Provide nothing beyond the component's own inputs — no provider setup for a dependency it doesn't inject; never `HttpTestingController`, never a real HTTP call.
- Prefer accessible roles/labels over test-id attributes.
- Group related cases under nested `describe('<behavior-area>', ...)` blocks (`rendering`, `interactions`).

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]


- **Wiring a mock "just in case" for a component that injects nothing**
  - Consequence: invents a dependency, adds noise, falsely implies coupling
  - Instead: provide only the component's inputs

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

# Check list

- [ ] The file is at `spec/{component-name}.component.spec.ts`
- [ ] No assertion reaches into `fixture.componentInstance` / `debugElement`
- [ ] The test provides only the component's inputs; no HTTP occurs
- [ ] Queries prefer roles/labels over test IDs

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a pure input/output component's input changes THEN the rendered DOM reflects the new value, with no provider/mock setup
- [ ] WHEN the user activates a control that emits an output THEN the `on:` handler is called with the expected payload

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]
