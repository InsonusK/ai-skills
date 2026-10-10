import { Route } from '@angular/router';
import { ORDERS_ROUTES } from './orders.routes';

// The feature exposes one parent route (providing its offline-sync replay
// handler in a route-level env injector) with the real screens as children.
const parent = ORDERS_ROUTES[0] as Route;
const children = (parent.children ?? []) as Route[];
const allRoutes = [parent, ...children];

describe('ORDERS_ROUTES', () => {
  it('registers the feature offline-sync provider in a route-level injector', () => {
    expect(parent.providers?.length).toBeGreaterThan(0);
  });

  it('never sets data.preload on its own routes — that decision belongs to the mounting point', () => {
    expect(allRoutes.filter((r) => r.data?.['preload'] !== undefined)).toEqual([]);
  });

  it('splits the heavy, rarely-visited report screen into its own loadComponent chunk', () => {
    const report = children.find((r) => r.path === 'report');
    expect(report?.loadComponent).toBeTypeOf('function');
    expect(report?.component).toBeUndefined();
  });

  it('keeps the main path bundled with the feature chunk (not further split)', () => {
    const main = children.find((r) => r.path === '');
    expect(main?.component).toBeTypeOf('function');
    expect(main?.loadComponent).toBeUndefined();
  });
});
