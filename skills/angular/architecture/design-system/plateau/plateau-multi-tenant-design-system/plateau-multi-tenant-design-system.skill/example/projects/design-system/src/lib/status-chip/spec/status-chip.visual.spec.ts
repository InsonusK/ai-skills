import { test, expect } from '@playwright/test';

test.describe('DsStatusChipComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/status-chip');
      await expect(page.locator('[data-preview="default"]')).toHaveScreenshot(`status-chip-default-${scheme}.png`);
    });
  }
});
