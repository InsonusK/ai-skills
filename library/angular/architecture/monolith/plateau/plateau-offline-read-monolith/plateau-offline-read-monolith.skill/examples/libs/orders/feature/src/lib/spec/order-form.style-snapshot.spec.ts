import { test, expect } from '@playwright/test';
import { readVisualStyleProperties } from './read-visual-style-properties';

test.describe('OrderFormComponent — style snapshot (paired with the visual spec)', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`computed style matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/order-form/idle');
      const styles = await readVisualStyleProperties(page.locator('ui-status-badge'));
      expect(JSON.stringify(styles, null, 2)).toMatchSnapshot(`order-form-idle-${scheme}.styles.txt`);
    });
  }
});
