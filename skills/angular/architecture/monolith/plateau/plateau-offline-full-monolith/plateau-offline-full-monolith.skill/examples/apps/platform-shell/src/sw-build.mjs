// Nx build step (nx:run-commands), run after `nx build platform-shell` writes
// the bundle to dist/. Not a webpack plugin — this workspace uses the
// esbuild-based @angular/build:application builder.
//
// It esbuild-bundles sw-src.ts (WebWorker target) then injects the precache
// manifest with workbox-build.
import { build } from 'esbuild';
import { injectManifest } from 'workbox-build';
import { fileURLToPath } from 'node:url';
import { dirname, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const outDir = resolve(here, '../../../dist/apps/platform-shell/browser');
const swTmp = resolve(outDir, 'sw-src.js');

await build({
  entryPoints: [resolve(here, 'sw-src.ts')],
  bundle: true,
  format: 'esm',
  platform: 'browser',
  target: 'es2022',
  outfile: swTmp,
  logLevel: 'warning',
});

const { count, size, warnings } = await injectManifest({
  swSrc: swTmp,
  swDest: resolve(outDir, 'sw.js'),
  globDirectory: outDir,
  globPatterns: ['**/*.{js,css,html}'], // app shell + lazy feature chunks
});

warnings.forEach((w) => console.warn(w));
console.log(`sw.js: precached ${count} files, ${(size / 1024).toFixed(1)} KiB`);
