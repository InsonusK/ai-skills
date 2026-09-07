---
name: plateau-offline-read-monolith--class-read-visual-style-properties
description: Shared helper that reads a fixed, curated set of visually meaningful computed CSS properties from a page element, for use by every spec/{component-name}.style-snapshot.spec.ts — offline-read-monolith plateau
domain: skill
type: template
whenToUse: when creating a project's read-visual-style-properties.ts helper, or extending the shared VISUAL_STYLE_PROPERTIES list
plateau: offline-read-monolith
artifact_type: module
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
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|read-visual-style-properties.ts.create]]


# How this generic file is used
Create one shared helper per plateau, not per component. Example locations:

- Platform plateau: `libs/{feature}/feature/testing/read-visual-style-properties.ts` (or `libs/shared/testing/read-visual-style-properties.ts` if shared across libraries)
- Design-system plateau: `projects/design-system/testing/read-visual-style-properties.ts`

It is imported by every `spec/{component-name}.style-snapshot.spec.ts` ([style-snapshot testing](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/style-snapshot-testing.md)) in both plateaus. Centralizing the property list here is what keeps the snapshot readable — per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/style-snapshot-approach.md|style-snapshot-approach]], a full computed-style dump is noisy; a shared, curated list is not.

# Goals

- Read `getComputedStyle()` values — actual resolved property values, never CSS class names — restricted to a single shared list of visually meaningful properties
- Keep that property list in exactly one place, so every component's style snapshot stays comparable and additions/removals to the list are a one-line change

# Implementation changes

```typescript
import type { Locator } from '@playwright/test';

export const VISUAL_STYLE_PROPERTIES = [
  'color',
  'backgroundColor',
  'borderColor',
  'borderWidth',
  'borderRadius',
  'boxShadow',
  'padding',
  'margin',
  'fontSize',
  'fontWeight',
  'lineHeight',
  'opacity',
  'display',
  'transform',
] as const;

export type VisualStyleSnapshot = Record<(typeof VISUAL_STYLE_PROPERTIES)[number], string>;

export async function readVisualStyleProperties(locator: Locator): Promise<VisualStyleSnapshot> {
  return locator.evaluate((el, properties) => {
    const computed = getComputedStyle(el);
    return Object.fromEntries(
      properties.map((property) => [property, computed[property as keyof CSSStyleDeclaration] as string]),
    ) as Record<string, string>;
  }, VISUAL_STYLE_PROPERTIES) as Promise<VisualStyleSnapshot>;
}
```

# Rule changes

## MUST
- `VISUAL_STYLE_PROPERTIES` must stay a single shared list imported by every style-snapshot spec — never redefined per component.
- The helper must be created in the project's test-support directory (e.g., `libs/{feature}/feature/testing/` or `projects/design-system/testing/`), not duplicated inside each component's `spec/` folder.
- The helper must read computed property values via `getComputedStyle()`, never a CSS class name or a Tailwind/utility-class string — a class name can stay unchanged while the value it resolves to changes.

## SHOULD
- Extend `VISUAL_STYLE_PROPERTIES` when a real regression class slips through undetected by the current list, rather than adding a component-specific one-off property read.

- **Reading `element.className` instead of `getComputedStyle()`** — Consequence: misses exactly the regression class this ADR exists to catch — a class name staying the same while its resolved CSS values change (e.g. a design token's value changes elsewhere in the stylesheet) — Instead: always resolve through `getComputedStyle()`
- **Defining a one-off property list per component** — Consequence: snapshots become incomparable across components, and the curation cost of "which properties matter" is paid over and over — Instead: extend the one shared `VISUAL_STYLE_PROPERTIES` list when a real gap is found
- **Copying the helper into every component's `spec/` folder** — Consequence: the property list drifts apart between components, and the curated, comparable snapshot format is lost — Instead: keep one shared helper per plateau and import it from the component spec via the workspace path alias or relative path
# Check list

- [ ] The helper is located in a project-level test-support directory, not duplicated per component
- [ ] Every style-snapshot spec imports `VISUAL_STYLE_PROPERTIES`/`readVisualStyleProperties` from this one shared helper
- [ ] No spec reads `className` or a raw `style` attribute string in place of `getComputedStyle()`

# Unittest TestCases

- [ ] WHEN `readVisualStyleProperties` is called on an element THEN it returns exactly the properties listed in `VISUAL_STYLE_PROPERTIES`, resolved to their computed values
- [ ] WHEN a CSS custom property changes a token's resolved color without touching the element's class name THEN the returned `color` value reflects the new resolved value
