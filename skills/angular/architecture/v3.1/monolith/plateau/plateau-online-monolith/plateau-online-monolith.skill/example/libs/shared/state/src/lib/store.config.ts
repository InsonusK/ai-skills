import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { provideStore, ActionReducerMap } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';

// The root store ships EMPTY at plateau-online-monolith. Slice-owning features
// (connectivity, notifications, auth) register their reducer + effects here later
// in the chain via mergeSliceProviders / additional provideState calls.
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface RootState {}
export const rootReducers: ActionReducerMap<RootState> = {};

export function provideGlobalStore(): EnvironmentProviders {
  return makeEnvironmentProviders([provideStore(rootReducers), provideEffects()]);
}
