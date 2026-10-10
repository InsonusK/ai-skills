import { test, expect } from '@playwright/test';

// Read resilience (VP4). Not executed where this example was built — the
// Playwright runner cannot fork workers in the sandbox — but kept as the
// intended shape of the offline-read acceptance test.
test('offline: the shell still loads, shows cached data and the offline banner', async ({
  page,
  context,
}) => {
  await page.route('**/api/orders', (route) =>
    route.fulfill({ status: 200, json: [{ id: '1', product_name: 'Widget', qty: 2, created_at: '2026-01-01T00:00:00Z' }] }),
  );
  await page.route('**/health', (route) => route.fulfill({ status: 200 }));

  // Warm the caches while online.
  await page.goto('/orders');
  await expect(page.getByText('Widget × 2')).toBeVisible();

  // Drop the network. The service worker serves the shell + last-known reads;
  // the health check fails, so the connectivity slice flips isOnline to false.
  await context.setOffline(true);
  await page.reload();

  await expect(page.getByRole('status')).toHaveText(/offline/i);
  await expect(page.getByText('Widget × 2')).toBeVisible();
});

test('offline: a mutation fails immediately — no queue in this plateau', async ({ page, context }) => {
  await page.route('**/api/orders', (route) =>
    route.request().method() === 'GET'
      ? route.fulfill({ status: 200, json: [] })
      : route.abort('internetdisconnected'),
  );
  await page.route('**/health', (route) => route.fulfill({ status: 200 }));

  await page.goto('/orders');
  await context.setOffline(true);

  await page.getByRole('textbox', { name: /product/i }).fill('Widget');
  await page.getByRole('spinbutton', { name: /quantity/i }).fill('2');
  await page.getByRole('button', { name: /add order/i }).click();

  // Surfaced as a failure (OfflineTransportError) — not queued.
  await expect(page.getByRole('alert')).toContainText(/network|offline|unreachable/i);
});
