import { computed, Injectable, signal } from '@angular/core';
import { SessionContract, SessionUser } from '@platform/contracts';

/**
 * The host's implementation of the shared SessionContract (solution-session-sharing).
 * In the real platform this is a thin read-only view over `libs/shared/state`'s
 * `auth` slice (selectCurrentUser / selectPermissions / selectIsLoggedIn). Here it
 * is a minimal signal-backed stand-in so the federation wiring is exercised.
 */
@Injectable({ providedIn: 'root' })
export class HostSession implements SessionContract {
  private readonly _user = signal<SessionUser | null>(null);
  private readonly _permissions = signal<readonly string[]>([]);

  readonly currentUser = this._user.asReadonly();
  readonly permissions = this._permissions.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  /** Called by the host's auth effects on Login Succeeded / Silent Refresh Succeeded. */
  setSession(user: SessionUser, permissions: readonly string[]): void {
    this._user.set(user);
    this._permissions.set(permissions);
  }

  /** Called on Session Expired / Logout. */
  clearSession(): void {
    this._user.set(null);
    this._permissions.set([]);
  }
}
