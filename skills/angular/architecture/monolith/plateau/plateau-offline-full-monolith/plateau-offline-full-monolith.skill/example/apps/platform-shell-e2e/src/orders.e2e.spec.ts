import { test, expect } from '@playwright/test';

test('shell redirects to /orders and an order can be created (API mocked)', async ({ page }) => {
  await page.route('**/api/orders', async (route) => {
    if (route.request().method() === 'POST') {
      await route.fulfill({ status: 201, json: { id: '1', product_name: 'Widget', qty: 2, created_at: '2026-01-01T00:00:00Z' } });
    } else {
      await route.fulfill({ status: 200, json: [] });
    }
  });

  await page.goto('/');
  await expect(page).toHaveURL(/\/orders$/);
  await page.getByRole('textbox', { name: /product/i }).fill('Widget');
  await page.getByRole('spinbutton', { name: /quantity/i }).fill('2');
  await page.getByRole('button', { name: /add order/i }).click();
  await expect(page.getByText('Widget × 2')).toBeVisible();
});
