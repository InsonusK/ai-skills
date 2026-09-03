---
description: Generic pattern for a computed-CSS-property snapshot spec under spec/, run alongside a component's pixel screenshot spec so a failing visual diff can be explained rather than just observed
project_name: "{demo-or-preview-app}"
name: "spec/{component-name}.style-snapshot"
element_kind: component
change_kind: create
tags:
  - solution/ui-testing
  - element/component-name-style-snapshot-spec-ts
---

# How this generic file is used
Created at `spec/{component-name}.style-snapshot.spec.ts` next to the component implementation. Applies identically in both catalogs (`solution-ui-testing` monolith / `solution-design-system-ui-testing` design system): navigate to the same demo/preview page a `.visual.spec.ts` already navigates to (see [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create]]), and assert a [computed-style snapshot](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/style-snapshot-testing.md) via [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create|read-visual-style-properties]]. Per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/style-snapshot-approach.md|style-snapshot-approach]].

# Goals

- Turn a failing pixel screenshot into a readable explanation of which specific CSS properties changed, and from what value to what value
- Give the agent/engineer a structured signal to check *before* running `--update-snapshots` on a failing visual spec, so a baseline is never accepted without understanding why it changed

# Implementation changes

```typescript
// File: projects/design-system/src/lib/ds-button/spec/ds-button.style-snapshot.spec.ts
import { test, expect } from '@playwright/test';
// The helper is a single shared file in the project's test-support directory
// (e.g. projects/design-system/testing/read-visual-style-properties.ts)
import { readVisualStyleProperties } from '@test/read-visual-style-properties';

test.describe('DsButtonComponent — style snapshot', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`computed style matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/ds-button/default');
      const styles = await readVisualStyleProperties(page.getByTestId('ds-button'));
      expect(JSON.stringify(styles, null, 2)).toMatchSnapshot(`ds-button-default-${scheme}.styles.txt`);
    });
  }

  test('computed style matches baseline (disabled)', async ({ page }) => {
    await page.goto('/ds-button/disabled');
    const styles = await readVisualStyleProperties(page.getByTestId('ds-button'));
    expect(JSON.stringify(styles, null, 2)).toMatchSnapshot('ds-button-disabled.styles.txt');
  });
});
```

# Rule changes

## MUST
- The file is created at `spec/{component-name}.style-snapshot.spec.ts`.
  - Risk: spec files next to `component.ts` clutter the component root and get mixed with source in reviews.
  - Fix: every test file for the component lives under its `spec/` folder.
- Text snapshot baselines (`*.styles.txt`) are committed under `spec/snapshot/`, via `snapshotPathTemplate` in `playwright.config.ts`.
  - Risk: snapshots left in Playwright's default `__snapshots__` folder scatter across the tree and are missed in review.
  - Fix: set `snapshotPathTemplate` so `toMatchSnapshot()` writes to `spec/snapshot/`.
- A style-snapshot spec covers exactly the same states and color schemes as the component's `.visual.spec.ts`.
  - Risk: if the two specs drift, a pixel diff has no paired style diff to explain it and the pairing loses its value.
  - Fix: mirror the visual spec's `for` loop and per-state tests one to one.
- Tests are grouped under a `test.describe('<component-name> — style snapshot', ...)` block.
  - Risk: a flat file gives no grouping in the Playwright report.
  - Fix: one `test.describe` per component.
- Properties are read only through the shared `readVisualStyleProperties` helper, never an ad hoc per-component list.
  - Risk: a component-specific property list makes snapshots incomparable across components and defeats the curated set.
  - Fix: import the one shared helper; extend its `VISUAL_STYLE_PROPERTIES` if a real gap is found.
- Before running `--update-snapshots` on a failing `.visual.spec.ts`, the paired style-snapshot diff is inspected first.
  - Risk: `--update-snapshots` updates both the PNG and the `.styles.txt` at once, so a real regression is baked into both baselines unseen.
  - Fix: empty style diff → pixel change is rendering noise, safe to accept; non-empty → confirm the named property change is intentional before updating.
- A style-snapshot spec is never treated as a replacement for the pixel screenshot spec.
  - Risk: the shared property list misses layout/paint issues outside it (`z-index` stacking, `overflow` clipping), so a green style snapshot gives false confidence.
  - Fix: both specs ship together; neither substitutes for the other.
## SHOULD
- **Running `--update-snapshots` on a failing visual spec without checking the paired style-snapshot diff** — Consequence: exactly [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create#Anti-patterns|the anti-pattern the visual spec already warns about]] — a real regression gets silently baked into the new baseline as "correct" — Instead: check the style-snapshot diff first; an empty diff means the pixel change is rendering noise, a non-empty diff names the exact property/value change to confirm as intentional (or reject as a regression)
- **Adding a component-specific property list instead of using the shared helper** — Consequence: snapshots become incomparable across components, and defeats the point of a curated, shared, readable property set — Instead: extend [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create|the one shared `VISUAL_STYLE_PROPERTIES` list]] if a real gap is found
- **Treating a passing style-snapshot as proof the component looks correct** — Consequence: the shared property list doesn't cover every possible visual regression (e.g. stacking/clipping issues) — over-trusting it alone misses what the pixel screenshot spec exists to catch — Instead: keep both specs running together; neither substitutes for the other

# Check list

- [ ] The file is created at `spec/{component-name}.style-snapshot.spec.ts`, not next to `component.ts`
- [ ] Text snapshots are committed under `spec/snapshot/`
- [ ] Every component/state with a `.visual.spec.ts` has a paired `.style-snapshot.spec.ts` covering the identical states and color schemes
- [ ] Every style-snapshot spec reads properties through the shared `readVisualStyleProperties` helper
- [ ] No `.visual.spec.ts` baseline was updated without first checking its paired style-snapshot diff
- [ ] Style-snapshot specs group tests under a `test.describe('<component-name> — style snapshot', ...)` block

# Unittest TestCases

- [ ] WHEN a CSS change alters a component's resolved `color` or `background-color` THEN
  - [ ] the style-snapshot spec fails with a diff naming the old and new value
- [ ] WHEN a pixel screenshot fails due to anti-aliasing/font-hinting differences only THEN
  - [ ] the paired style-snapshot spec still passes, signaling the pixel diff is rendering noise rather than a real regression
- [ ] WHEN a component's dark-mode `light-dark()` branch stops applying THEN
  - [ ] the dark-scheme style-snapshot fails, naming the specific properties that no longer resolve to their dark-mode values
