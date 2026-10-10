import { test, expect } from '@playwright/test';

test.describe('VP8 — persisted state', () => {
  test('a half-filled order form survives a page reload', async ({ page }) => {
    await page.route('**/api/orders', (route) => route.fulfill({ status: 200, json: [] }));

    await page.goto('/orders');
    await page.getByRole('textbox', { name: /product/i }).fill('Widget');
    await page.getByRole('spinbutton', { name: /quantity/i }).fill('5');

    // reload — the in-memory component state is gone, the persisted draft is not
    await page.reload();

    await expect(page.getByRole('textbox', { name: /product/i })).toHaveValue('Widget');
    await expect(page.getByRole('spinbutton', { name: /quantity/i })).toHaveValue('5');
  });

  test('the draft clears after a successful submit', async ({ page }) => {
    await page.route('**/api/orders', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          json: { id: '1', product_name: 'Widget', qty: 2, created_at: '2026-01-01T00:00:00Z' },
        });
      } else {
        await route.fulfill({ status: 200, json: [] });
      }
    });

    await page.goto('/orders');
    await page.getByRole('textbox', { name: /product/i }).fill('Widget');
    await page.getByRole('spinbutton', { name: /quantity/i }).fill('2');
    await page.getByRole('button', { name: /add order/i }).click();
    await expect(page.getByText('Widget × 2')).toBeVisible();

    await page.reload();
    await expect(page.getByRole('textbox', { name: /product/i })).toHaveValue('');
  });
});
