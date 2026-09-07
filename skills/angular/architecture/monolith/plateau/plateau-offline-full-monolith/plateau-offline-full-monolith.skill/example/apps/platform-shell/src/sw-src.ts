/// <reference lib="webworker" />
// Runtime service worker. Compiled with tsconfig.sw.json (WebWorker lib, no
// DOM), then processed by sw-build.mjs (workbox-build injectManifest) into
// dist/apps/platform-shell/sw.js. Not part of the app bundle, not run by Vitest.

import { precacheAndRoute } from 'workbox-precaching';
import { registerRoute } from 'workbox-routing';
import { CacheFirst, NetworkOnly, StaleWhileRevalidate } from 'workbox-strategies';
import { ExpirationPlugin } from 'workbox-expiration';
import { isApiRead, isNetworkOnly, isStaticAsset } from './sw-routes';

declare const self: ServiceWorkerGlobalScope & { __WB_MANIFEST: Array<{ url: string; revision: string | null }> };

// 1. App shell + lazy feature chunks — precache (atomically updated bundle).
precacheAndRoute(self.__WB_MANIFEST);

// 4. Auth + every non-GET request — network-only. Registered FIRST so a
//    mutation can never be matched by the API-reads rule below.
registerRoute(
  ({ url, request }) => isNetworkOnly({ url, method: request.method, destination: request.destination }),
  new NetworkOnly(),
);

// 2. Static design-system assets — cache-first.
registerRoute(
  ({ url, request }) => isStaticAsset({ url, method: request.method, destination: request.destination }),
  new CacheFirst({ cacheName: 'static-assets', plugins: [new ExpirationPlugin({ maxEntries: 200 })] }),
);

// 3. API GET reads — stale-while-revalidate (last-known data offline).
registerRoute(
  ({ url, request }) => isApiRead({ url, method: request.method, destination: request.destination }),
  new StaleWhileRevalidate({ cacheName: 'api-reads' }),
);
