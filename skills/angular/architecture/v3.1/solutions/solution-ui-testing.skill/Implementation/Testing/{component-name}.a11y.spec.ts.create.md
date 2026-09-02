---
description: Generic pattern for an @axe-core/playwright accessibility scan under spec/, targeting a component's demo/preview page
project_name: "{demo-or-preview-app}"
name: "spec/{component-name}.a11y"
element_kind: component
change_kind: create
tags:
  - solution/ui-testing
  - element/component-name-a11y-spec-ts
---

# How this generic file is used
Created at `spec/{component-name}.a11y.spec.ts` next to the component implementation. Applies identically to both plateaus this solution covers, reusing the same demo/preview page a visual spec already navigates to for [accessibility testing](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/accessibility-testing.md) (see [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create]]). Per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach.md|accessibility-testing-approach]].

# Goals

- Automatically catch WCAG-level accessibility violations (contrast, ARIA validity, landmark/heading structure) that neither a behavioral component test nor a visual screenshot test can detect

# Implementation changes

```typescript
// File: projects/design-system/src/lib/ds-button/spec/ds-button.a11y.spec.ts
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('DsButtonComponent — accessibility', () => {
  test('has no automatically detectable violations (default)', async ({ page }) => {
    await page.goto('/ds-button/default');
    const results = await new AxeBuilder({ page }).analyze();
    expect(results.violations).toEqual([]);
  });

  test('has no automatically detectable violations (disabled)', async ({ page }) => {
    await page.goto('/ds-button/disabled');
    const results = await new AxeBuilder({ page }).analyze();
    expect(results.violations).toEqual([]);
  });
});
```

# Rule changes

## MUST
- The file MUST be created at `spec/{component-name}.a11y.spec.ts` so all test files live under `spec/` and do not clutter the component directory root.
- An a11y spec MUST navigate directly to the component's stable demo/preview URL, the same one used by its visual spec.
- Every meaningfully distinct state the component's demo/preview page exposes MUST have its own axe-core scan, since a state change (e.g. `disabled`, an error message appearing) can introduce a violation absent from the default state.
- Tests in an a11y spec MUST be grouped under a `test.describe('<component-name> — accessibility', () => { ... })` block.
- Any explicit, individually justified exception to a specific axe rule MUST be scoped to that one rule and documented inline with the reason — never a blanket disable of the whole scan.

- an a11y spec must never be treated as a complete accessibility audit — it catches the mechanically-checkable subset of WCAG rules only; issues requiring human judgment (meaningful alt text, sensible focus order) still need occasional manual review.
## SHOULD
- **Disabling axe-core entirely for a component because one rule produces a false positive** — Consequence: loses coverage for every other rule the scan would have caught, for the sake of silencing one — Instead: scope the exception to the specific rule ID, with a documented reason
- **Treating a passing a11y spec as proof the component is fully accessible** — Consequence: real, non-mechanically-detectable issues (misleading alt text, confusing focus order) go unnoticed because the automated check passed — Instead: treat this as a regression net for the mechanically-checkable subset, not a substitute for occasional manual/expert review

# Check list

- [ ] The file is created at `spec/{component-name}.a11y.spec.ts`, not next to `component.ts`
- [ ] Every component/state with a demo/preview page has a corresponding axe-core scan
- [ ] No scan is disabled wholesale — any rule exception is scoped and documented
- [ ] CI fails the build on any unexpected violation, not just a warning
- [ ] A11y specs group tests under a `test.describe('<component-name> — accessibility', ...)` block

# Unittest TestCases

- [ ] WHEN a component's contrast ratio drops below the WCAG-required threshold THEN
  - [ ] the a11y spec fails with a `color-contrast` violation
- [ ] WHEN a form control loses its associated label THEN
  - [ ] the a11y spec fails with a `label`/`aria-*` violation
