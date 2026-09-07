import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { provideState, provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { connectivityFeature } from './connectivity/connectivity.reducer';
import { ConnectivityEffects } from './connectivity/connectivity.effects';
import { notificationsFeature } from './notifications/notifications.reducer';
import { authFeature } from './auth/auth.reducer';
import { AuthEffects } from './auth/auth.effects';

// The root store itself is empty — cross-cutting state lives in feature slices
// registered alongside it: `connectivity` (VP4), `notifications` (VP5),
// `auth` (VP7).
export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideStore({}),
    provideState(connectivityFeature),
    provideState(notificationsFeature),
    provideState(authFeature),
    provideEffects(ConnectivityEffects, AuthEffects),
  ]);
}
