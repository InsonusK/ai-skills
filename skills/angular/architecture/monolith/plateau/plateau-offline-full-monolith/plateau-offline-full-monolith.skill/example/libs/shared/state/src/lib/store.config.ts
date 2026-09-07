import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { provideState, provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { connectivityFeature } from './connectivity/connectivity.reducer';
import { ConnectivityEffects } from './connectivity/connectivity.effects';
import { notificationsFeature } from './notifications/notifications.reducer';

// The root store itself is still empty — cross-cutting state lives in feature
// slices registered alongside it. `connectivity` (VP4 / solution-offline-first),
// then `notifications` (VP5 / solution-offline-sync); `auth` (VP7) later.
export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideStore({}),
    provideState(connectivityFeature),
    provideState(notificationsFeature),
    provideEffects(ConnectivityEffects),
  ]);
}
