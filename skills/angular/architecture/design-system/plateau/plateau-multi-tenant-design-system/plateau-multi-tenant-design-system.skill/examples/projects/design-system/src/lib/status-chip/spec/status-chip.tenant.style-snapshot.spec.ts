import { test, expect } from '@playwright/test';
import { readVisualStyleProperties } from '../../../../testing/read-visual-style-properties';

// Multi-tenant theming (design-system VP1): the same component, same DOM, only the
// token *values* change under `:root[data-tenant='<id>']`. Typography/density are
// unchanged — a tenant varies colour only.
const TENANTS = ['acme', 'globex'] as const;

test.describe('DsStatusChipComponent — per-tenant style snapshot', () => {
  for (const tenant of TENANTS) {
    for (const scheme of ['light', 'dark'] as const) {
      test(`in-progress chip matches baseline (${tenant} / ${scheme})`, async ({ page }) => {
        await page.emulateMedia({ colorScheme: scheme });
        await page.goto('/status-chip');
        await page.getByTestId('tenant-select').selectOption(tenant);
        const styles = await readVisualStyleProperties(
          page.locator('[data-preview="default"] .ds-status-chip--in-progress'),
        );
        expect(JSON.stringify(styles, null, 2)).toMatchSnapshot(
          `status-chip-in-progress-${tenant}-${scheme}.styles.txt`,
        );
      });
    }
  }

  test('font token is identical across tenants — a tenant varies colour only', async ({ page }) => {
    await page.goto('/status-chip');
    const chip = page.locator('[data-preview="default"] .ds-status-chip--in-progress');

    await page.getByTestId('tenant-select').selectOption('acme');
    const acmeFont = await chip.evaluate((el) => getComputedStyle(el).font);

    await page.getByTestId('tenant-select').selectOption('globex');
    const globexFont = await chip.evaluate((el) => getComputedStyle(el).font);

    expect(globexFont).toBe(acmeFont);
  });
});
