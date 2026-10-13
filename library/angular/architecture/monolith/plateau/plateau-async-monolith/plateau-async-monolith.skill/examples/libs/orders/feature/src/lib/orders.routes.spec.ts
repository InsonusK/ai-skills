import { Route } from '@angular/router';
import { ORDERS_ROUTES } from './orders.routes';

describe('ORDERS_ROUTES', () => {
  it('never sets data.preload on its own routes — that decision belongs to the mounting point', () => {
    const flagged = (ORDERS_ROUTES as Route[]).filter((r) => r.data?.['preload'] !== undefined);
    expect(flagged).toEqual([]);
  });

  it('splits the heavy, rarely-visited report screen into its own loadComponent chunk', () => {
    const report = (ORDERS_ROUTES as Route[]).find((r) => r.path === 'report');
    expect(report?.loadComponent).toBeTypeOf('function');
    expect(report?.component).toBeUndefined();
  });

  it('keeps the main path bundled with the feature chunk (not further split)', () => {
    const main = (ORDERS_ROUTES as Route[]).find((r) => r.path === '');
    expect(main?.component).toBeTypeOf('function');
    expect(main?.loadComponent).toBeUndefined();
  });
});
