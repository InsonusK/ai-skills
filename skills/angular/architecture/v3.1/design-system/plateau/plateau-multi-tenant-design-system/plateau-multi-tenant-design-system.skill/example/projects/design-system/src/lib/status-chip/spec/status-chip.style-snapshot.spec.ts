import { test, expect } from '@playwright/test';
import { readVisualStyleProperties } from '../../../../testing/read-visual-style-properties';

test.describe('DsStatusChipComponent — style snapshot', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`computed style matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/status-chip');
      const styles = await readVisualStyleProperties(
        page.locator('[data-preview="default"] .ds-status-chip--in-progress'),
      );
      expect(JSON.stringify(styles, null, 2)).toMatchSnapshot(`status-chip-in-progress-${scheme}.styles.txt`);
    });
  }
});
