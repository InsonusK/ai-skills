import type { Locator } from '@playwright/test';

/**
 * The single shared, curated list of visually meaningful computed CSS properties.
 * Imported by every spec/{component}.style-snapshot.spec.ts — never redefined per component.
 */
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
    const computed = getComputedStyle(el as Element);
    return Object.fromEntries(
      properties.map((property) => [property, computed[property as keyof CSSStyleDeclaration] as string]),
    ) as Record<string, string>;
  }, VISUAL_STYLE_PROPERTIES) as Promise<VisualStyleSnapshot>;
}
