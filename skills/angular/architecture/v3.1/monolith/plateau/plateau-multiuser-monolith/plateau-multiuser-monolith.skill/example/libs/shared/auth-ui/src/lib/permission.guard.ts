import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectPermissions } from '@org/shared-state';

/**
 * `requirePermission('orders.delete')` — a functional guard checking a
 * permission string against the auth slice. Attached at the feature's OWN route
 * (hierarchical ownership), never centralised in the shell. A failed check
 * redirects to `/forbidden`.
 */
export function requirePermission(permission: string): CanActivateFn & CanMatchFn {
  return () => {
    const permissions = inject(Store).selectSignal(selectPermissions)();
    const router = inject(Router);
    return permissions.includes(permission) || router.createUrlTree(['/forbidden']);
  };
}
