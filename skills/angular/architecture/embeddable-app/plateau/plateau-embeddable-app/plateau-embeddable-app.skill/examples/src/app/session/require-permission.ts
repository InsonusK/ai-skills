import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { SESSION_CONTRACT } from '@platform/contracts';

/**
 * Reads the SessionContract the host published (solution-session-sharing).
 * The remote NEVER implements its own login — an unauthenticated session renders
 * a not-authenticated state; the host owns any redirect.
 */
export function requirePermission(permission: string): CanActivateFn {
  return () => {
    const session = inject(SESSION_CONTRACT);
    if (!session.isAuthenticated()) return false;
    return session.permissions().includes(permission);
  };
}
