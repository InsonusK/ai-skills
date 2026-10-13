import { TestBed } from '@angular/core/testing';
import { provideRouter, UrlTree } from '@angular/router';
import { provideMockStore } from '@ngrx/store/testing';
import { selectPermissions } from '@org/shared-state';
import { requirePermission } from './permission.guard';

function evaluate(permissions: string[]): boolean | UrlTree {
  TestBed.configureTestingModule({
    providers: [
      provideRouter([]),
      provideMockStore({ selectors: [{ selector: selectPermissions, value: permissions }] }),
    ],
  });
  return TestBed.runInInjectionContext(
    () => requirePermission('orders.archive')({} as never, {} as never) as boolean | UrlTree,
  );
}

describe('requirePermission', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('allows navigation when the permission is present', () => {
    expect(evaluate(['orders.archive', 'orders.view'])).toBe(true);
  });

  it('redirects to /forbidden when the permission is missing', () => {
    const result = evaluate(['orders.view']);
    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/forbidden');
  });
});
