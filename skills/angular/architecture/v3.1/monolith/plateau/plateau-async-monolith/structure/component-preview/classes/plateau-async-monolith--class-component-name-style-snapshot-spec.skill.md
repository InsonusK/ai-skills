---
name: plateau-async-monolith--class-component-name-style-snapshot-spec
description: Generic pattern for a computed-CSS-property snapshot spec under spec/, run alongside a component's pixel screenshot spec so a failing visual diff can be explained rather than just observed — async-monolith plateau
domain: skill
type: template
whenToUse: when creating or editing this class in the async-monolith plateau, or another artifact that plays the same role
plateau: async-monolith
artifact_type: spec
version: 20260902160000
tags:
  - skill/template/class
  - plateau/async-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
---

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|{component-name}.style-snapshot.spec.ts.create]]


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
- The file must be created at `spec/{component-name}.style-snapshot.spec.ts` so all test files live under `spec/` and do not clutter the component directory root.
- Text snapshot baselines (`*.styles.txt`) must be committed under `spec/snapshot/` next to the spec. Configure `snapshotPathTemplate` in `playwright.config.ts` so that `toMatchSnapshot()` stores text snapshots in `spec/snapshot/` rather than the default `__snapshots__` folder.
- A style-snapshot spec must cover exactly the same states (and color schemes) as the component's `.visual.spec.ts` — the two specs stay paired, one per state.
- Tests in a style-snapshot spec must be grouped under a `test.describe('<component-name> — style snapshot', () => { ... })` block.
- A style-snapshot spec must read properties only through [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create|the shared `readVisualStyleProperties` helper]], never a component-specific ad hoc property list.
- Before updating a failing `.visual.spec.ts` baseline (`--update-snapshots`), the corresponding style-snapshot diff must be inspected first: no property change means the pixel diff is rendering noise (safe to accept); a property change means the diff itself states what moved, and that change must be confirmed intentional before either snapshot is updated. Note: `--update-snapshots` is the single Playwright flag that updates both the PNG baseline and the paired `.styles.txt` text snapshot; both files are updated together.

- a style-snapshot spec must never be treated as a replacement for the pixel screenshot spec — it does not catch layout/paint issues that fall outside the shared property list (e.g. `z-index` stacking, `overflow` clipping); both specs ship together.
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
