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
- The file is created at `spec/{component-name}.a11y.spec.ts`.
  - Risk: spec files next to `component.ts` clutter the component root and get mixed with source in reviews.
  - Fix: every test file for the component lives under its `spec/` folder.
- An a11y spec navigates directly to the component's stable demo/preview URL — the same one its visual spec uses.
  - Risk: reaching the component through app navigation scans unrelated page content and reports violations outside the component.
  - Fix: `page.goto('/<component>/<state>')` against the preview app's stable route.
- Every meaningfully distinct state the demo page exposes has its own axe-core scan.
  - Risk: a `disabled` state or an appearing error message can introduce a violation the default-state scan never sees.
  - Fix: one `test` + `AxeBuilder().analyze()` per state.
- Tests are grouped under a `test.describe('<component-name> — accessibility', ...)` block.
  - Risk: a flat file gives no grouping in the Playwright report.
  - Fix: one `test.describe` per component.
- Any axe-rule exception is scoped to that one rule and documented inline with the reason — never a blanket disable.
  - Risk: `.disableRules()` with a broad list or a skipped scan drops coverage for every other rule to silence one.
  - Fix: `.disableRules(['color-contrast'])` with a comment stating why, or better, fix the violation.
- An a11y spec is never treated as a complete accessibility audit.
  - Risk: a green mechanical scan gives false confidence while misleading alt text or a broken focus order ships.
  - Fix: keep it as a regression net for the checkable subset; schedule occasional manual/expert review.
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
