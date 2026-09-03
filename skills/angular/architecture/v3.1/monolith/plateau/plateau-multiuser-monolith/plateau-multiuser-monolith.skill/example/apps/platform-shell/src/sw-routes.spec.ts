import { isApiRead, isNetworkOnly, isStaticAsset, RouteInput } from './sw-routes';

const at = (path: string, method = 'GET', destination = ''): RouteInput => ({
  url: new URL(path, 'https://app.example'),
  method,
  destination,
});

describe('service worker routing predicates', () => {
  it('routes images and fonts to the static-asset (cache-first) rule', () => {
    expect(isStaticAsset(at('/assets/logo.svg', 'GET', 'image'))).toBe(true);
    expect(isStaticAsset(at('/assets/Inter.woff2', 'GET', 'font'))).toBe(true);
    expect(isStaticAsset(at('/api/orders', 'GET', ''))).toBe(false);
  });

  it('routes API GET reads to stale-while-revalidate', () => {
    expect(isApiRead(at('/api/orders', 'GET'))).toBe(true);
  });

  it('routes auth endpoints and every non-GET request to network-only', () => {
    expect(isNetworkOnly(at('/auth/token', 'GET'))).toBe(true);
    expect(isNetworkOnly(at('/api/orders', 'POST'))).toBe(true);
    expect(isNetworkOnly(at('/api/orders', 'PUT'))).toBe(true);
    expect(isNetworkOnly(at('/api/orders', 'DELETE'))).toBe(true);
  });

  it('never lets a mutation fall through to the API-reads rule', () => {
    const post = at('/api/orders', 'POST');
    expect(isNetworkOnly(post)).toBe(true);
    expect(isApiRead(post)).toBe(false);
  });
});
