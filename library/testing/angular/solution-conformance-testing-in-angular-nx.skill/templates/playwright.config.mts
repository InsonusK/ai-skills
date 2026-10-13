import { defineConfig } from '@playwright/test';
import { nxE2EPreset } from '@nx/playwright/preset';
import { workspaceRoot } from '@nx/devkit';
import { resolve } from 'node:path';

// `make test-kind-ui` passes the directory this run writes to and a free port for the
// application server. Run directly (`nx e2e {E2eProject}`) it falls back to tmp/ and 4200.
const kindDir = process.env['TEST_KIND_DIR'] ?? resolve(workspaceRoot, 'tmp/e2e/{E2eProject}');
const port = process.env['UI_TEST_PORT'] ?? '4200';
const baseURL = `http://127.0.0.1:${port}`;

export default defineConfig({
  ...nxE2EPreset(import.meta.filename, { testDir: './src' }),
  testMatch: ['**/*.ui.spec.ts', '**/*.visual.spec.ts', '**/*.style-snapshot.spec.ts', '**/*.a11y.spec.ts'],
  outputDir: resolve(kindDir, 'report/ui/artifacts'),
  snapshotPathTemplate: '{testDir}/__screenshots__/{testFileName}/{arg}{ext}',
  updateSnapshots: 'none',
  forbidOnly: true,
  retries: 0,
  workers: 1,
  reporter: [
    ['list'],
    ['json', { outputFile: resolve(kindDir, 'result/ui.native.json') }],
    ['html', { outputFolder: resolve(kindDir, 'report/ui/playwright'), open: 'never' }],
  ],
  use: {
    browserName: 'chromium',
    baseURL,
    viewport: { width: 1280, height: 720 },
    locale: 'en-US',
    timezoneId: 'UTC',
    colorScheme: 'light',
    reducedMotion: 'reduce',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  expect: { toHaveScreenshot: { animations: 'disabled' } },
  webServer: {
    command: `npx nx run {HostProject}:serve --host 127.0.0.1 --port ${port}`,
    url: baseURL,
    reuseExistingServer: false,
    cwd: workspaceRoot,
    timeout: 120_000,
  },
});
