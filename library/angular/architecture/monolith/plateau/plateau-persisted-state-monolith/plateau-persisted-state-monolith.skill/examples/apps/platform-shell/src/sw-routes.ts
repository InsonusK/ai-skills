/**
 * The content-type routing predicates for the Workbox service worker, kept
 * in a plain DOM-safe module so they can be unit-tested without a service
 * worker context. `sw-src.ts` imports these and wires them to Workbox
 * strategies; the spec asserts a non-GET request always resolves to
 * network-only, never to the API-reads (stale-while-revalidate) rule.
 */

export interface RouteInput {
  readonly url: URL;
  readonly method: string;
  /** `request.destination` — 'image' | 'font' | 'script' | 'document' | '' ... */
  readonly destination: string;
}

/** Rule 2 — static design-system assets: cache-first. */
export function isStaticAsset({ destination }: RouteInput): boolean {
  return destination === 'image' || destination === 'font';
}

/** Rule 4 — auth endpoints and every non-GET request: network-only, never cached. */
export function isNetworkOnly({ url, method }: RouteInput): boolean {
  return url.pathname.startsWith('/auth/') || method.toUpperCase() !== 'GET';
}

/** Rule 3 — API GET reads: stale-while-revalidate. Must never match a mutation. */
export function isApiRead(input: RouteInput): boolean {
  return (
    input.url.pathname.startsWith('/api/') &&
    input.method.toUpperCase() === 'GET' &&
    !isNetworkOnly(input)
  );
}
