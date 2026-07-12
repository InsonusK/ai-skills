---
description: Generic pattern for a Playwright screenshot-regression spec against a component's demo/preview page, in both light and dark color schemes
project_name: "{demo-or-preview-app}"
name: "{component-name}.visual"
element_kind: component
change_kind: create
---

# How this generic file is used
Applies identically to both plateaus this solution covers: navigate to the component's existing demo/preview page (design system: `projects/demo`, per [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/DesignSystemComponents/demo.project.extend]]; platform: `apps/component-preview`, per [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create]]) and assert a screenshot match. Per [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/visual-regression-approach]] — no Storybook, no Chromatic.

# Goals

- Catch layout, dark-mode, contrast, and CSS-specificity regressions that a behavioral component test (jsdom/happy-dom, no layout engine) cannot detect

# Implementation changes

```typescript
import { test, expect } from '@playwright/test';

test.describe('DsButtonComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/ds-button/default');
      await expect(page).toHaveScreenshot(`ds-button-default-${scheme}.png`);
    });
  }

  test('matches baseline (disabled)', async ({ page }) => {
    await page.goto('/ds-button/disabled');
    await expect(page).toHaveScreenshot('ds-button-disabled.png');
  });
});
```

# Rule changes

## MUST
- A visual spec MUST navigate directly to the component's stable demo/preview URL — it MUST NOT drive the UI through unrelated navigation to reach it.
- Every component that ships both a light and a dark rendering path MUST be screenshotted in both `page.emulateMedia({ colorScheme })` states.
- Every meaningfully distinct state the component's demo/preview page exposes (default, disabled, error, loading, etc.) MUST have its own baseline screenshot.
- A baseline screenshot MUST only be updated (`--update-snapshots`) as a deliberate, reviewed part of a PR that intentionally changes the component's appearance — never to silence an unexplained CI failure.

## MUST NOT
- A visual spec MUST NOT assert against a page that also renders unrelated, unrelated-to-the-test dynamic content (e.g. a live clock, randomly-ordered data) that would make the screenshot inherently flaky.

# Anti-patterns

- **Updating a baseline screenshot without understanding why it changed**
  - Consequence: silently accepts a real visual regression as the new "correct" baseline
  - Instead: investigate the diff first; update the baseline only once the change is confirmed intentional

- **Screenshotting a page with non-deterministic content (timestamps, random ordering, animation mid-flight)**
  - Consequence: flaky CI failures unrelated to any real regression, eroding trust in the whole suite
  - Instead: use fixed example data in the demo/preview page, and disable/complete animations before capturing (Playwright's `toHaveScreenshot` has built-in animation-handling options)

# Check list

- [ ] Every component/state with a demo/preview page has at least one committed baseline screenshot
- [ ] Both light and dark color schemes are covered for any component with a dark-mode-aware rendering path
- [ ] No baseline was updated without a reviewed, intentional visual change in the same PR

# Unittest TestCases

- [ ] WHEN a component's CSS is changed such that its rendered layout shifts THEN
  - [ ] the visual spec fails with a pixel diff against the committed baseline
- [ ] WHEN a component's `light-dark()` dark-mode branch stops applying THEN
  - [ ] the dark-scheme screenshot fails against its baseline, even though the light-scheme screenshot still passes
