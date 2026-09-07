import { defineConfig } from '@playwright/test';

/**
 * Federation smoke test: boot the remote, then the host, and assert the host
 * mounts the remote's exposed './Routes' at /reports and shares ONE session.
 *
 * NOTE: written and configured; not executed where this example was built (the
 * sandbox cannot run two dev servers + the federation runtime). Run locally:
 *   (cd ../embeddable-app && npx ng serve --port 4401) &
 *   npx ng serve --port 4400 &
 *   npx playwright test
 */
export default defineConfig({
  testDir: './e2e',
  use: { baseURL: 'http://localhost:4400' },
  webServer: [
    { command: 'cd ../embeddable-app && npx ng serve --port 4401', url: 'http://localhost:4401', reuseExistingServer: true, timeout: 120_000 },
    { command: 'npx ng serve --port 4400', url: 'http://localhost:4400', reuseExistingServer: true, timeout: 120_000 },
  ],
});
