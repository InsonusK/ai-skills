import { InjectionToken, Signal } from '@angular/core';

/** A user, as every consumer of the platform sees it. Never a role — only permission strings. */
export interface SessionUser {
  readonly id: string;
  readonly displayName: string;
}

/**
 * A read-only, signal-shaped view of the host's auth slice. A remote reads this;
 * only the host (via solution-session-sharing) ever provides an implementation.
 */
export interface SessionContract {
  readonly currentUser: Signal<SessionUser | null>;
  readonly permissions: Signal<readonly string[]>;
  readonly isAuthenticated: Signal<boolean>;
}

export const SESSION_CONTRACT = new InjectionToken<SessionContract>('platform.SessionContract');
