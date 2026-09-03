import { test, expect } from '@playwright/test';

// Written and configured; Playwright's runner cannot fork workers in this
// sandbox, so baselines under spec/snapshot/ are generated where CI runs.
test.describe('DsButtonComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/button');
      await expect(page.locator('[data-preview="default"]')).toHaveScreenshot(`button-default-${scheme}.png`);
    });
  }

  test('matches baseline (disabled)', async ({ page }) => {
    await page.goto('/button');
    await expect(page.locator('[data-preview="disabled"]')).toHaveScreenshot('button-disabled.png');
  });
});
