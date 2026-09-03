---
name: plateau-design-system--class-component-name-a11y-spec
description: Generic @axe-core/playwright accessibility scan for a ds-* component at spec/{component}.a11y.spec.ts, targeting its projects/demo preview page — design-system plateau
domain: skill
type: template
whenToUse: when writing or reviewing a ds-* component's spec/{component}.a11y.spec.ts
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

> `projects/design-system/src/lib/{component}/spec/{component}.a11y.spec.ts`. Reuses the same preview page the visual spec navigates to.

# Goal

- Automatically catch WCAG-level violations (contrast, ARIA validity, landmark/heading structure) that neither a behavioural nor a visual test detects

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|Testing/{component-name}.a11y.spec.ts.create]]

# Implementation

```typescript
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('DsButtonComponent — accessibility', () => {
  test('has no automatically detectable violations (default)', async ({ page }) => {
    await page.goto('/button');
    const results = await new AxeBuilder({ page }).include('[data-preview="default"]').analyze();
    expect(results.violations).toEqual([]);
  });
});
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|Testing/{component-name}.a11y.spec.ts.create]]

# Rules

## MUST
- The file is `spec/{component-name}.a11y.spec.ts`.
- Navigate directly to the component's stable preview URL, the same one the visual spec uses.
- Every meaningfully distinct state has its own axe scan — a state change can introduce a violation absent from the default.
- Group tests under `test.describe('<component-name> — accessibility', ...)`.
- Any exception to an axe rule is scoped to that one rule and documented inline — never a blanket disable of the whole scan.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|Testing/{component-name}.a11y.spec.ts.create]]


- **Treating a passing a11y spec as proof the component is fully accessible**
  - Consequence: non-mechanically-detectable issues (misleading labels, confusing focus order) go unnoticed
  - Instead: a regression net for the mechanical subset, not a substitute for occasional expert review

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|Testing/{component-name}.a11y.spec.ts.create]]

# Check list

- [ ] The file is at `spec/{component-name}.a11y.spec.ts`
- [ ] Every component/state with a preview page has a corresponding axe scan
- [ ] No scan is disabled wholesale — any rule exception is scoped and documented
- [ ] Tests grouped under `test.describe('<component-name> — accessibility', ...)`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|Testing/{component-name}.a11y.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a component's contrast ratio drops below the WCAG threshold THEN the a11y spec fails with a `color-contrast` violation
- [ ] WHEN a form control loses its associated label THEN the a11y spec fails with a `label`/`aria-*` violation

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|Testing/{component-name}.a11y.spec.ts.create]]
