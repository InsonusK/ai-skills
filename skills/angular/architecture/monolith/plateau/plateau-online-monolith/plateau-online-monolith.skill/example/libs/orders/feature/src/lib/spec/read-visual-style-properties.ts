import type { Locator } from '@playwright/test';

export const VISUAL_STYLE_PROPERTIES = [
  'color', 'backgroundColor', 'borderColor', 'borderWidth', 'borderRadius',
  'boxShadow', 'padding', 'margin', 'fontSize', 'fontWeight', 'lineHeight',
  'opacity', 'display', 'transform',
] as const;

export type VisualStyleSnapshot = Record<(typeof VISUAL_STYLE_PROPERTIES)[number], string>;

export async function readVisualStyleProperties(locator: Locator): Promise<VisualStyleSnapshot> {
  return locator.evaluate((el, props) => {
    const c = getComputedStyle(el as Element);
    return Object.fromEntries(props.map((p) => [p, c[p as keyof CSSStyleDeclaration] as string]));
  }, VISUAL_STYLE_PROPERTIES) as Promise<VisualStyleSnapshot>;
}
