import { test, expect } from '@playwright/test';
import { readVisualStyleProperties } from '../../../../testing/read-visual-style-properties';

test.describe('DsButtonComponent — style snapshot', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`computed style matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/button');
      const styles = await readVisualStyleProperties(
        page.locator('[data-preview="default"] [data-testid="ds-button"]'),
      );
      expect(JSON.stringify(styles, null, 2)).toMatchSnapshot(`button-default-${scheme}.styles.txt`);
    });
  }

  test('computed style matches baseline (disabled)', async ({ page }) => {
    await page.goto('/button');
    const styles = await readVisualStyleProperties(
      page.locator('[data-preview="disabled"] [data-testid="ds-button"]'),
    );
    expect(JSON.stringify(styles, null, 2)).toMatchSnapshot('button-disabled.styles.txt');
  });
});
