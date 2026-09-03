import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { provideState, provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { connectivityFeature } from './connectivity/connectivity.reducer';
import { ConnectivityEffects } from './connectivity/connectivity.effects';

// The root store itself is still empty — cross-cutting state lives in feature
// slices registered alongside it. `connectivity` (VP4 / solution-offline-first)
// is the first; `notifications` and `auth` arrive further down the chain.
export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideStore({}),
    provideState(connectivityFeature),
    provideEffects(ConnectivityEffects),
  ]);
}
