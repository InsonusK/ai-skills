import { createFeature, createReducer, createSelector, on } from '@ngrx/store';
import { ConnectivityActions } from './connectivity.actions';

export interface ConnectivityState {
  /** the browser's own `navigator.onLine` signal */
  readonly browserOnline: boolean;
  /** the result of the most recent backend health check */
  readonly lastHealthCheckOk: boolean;
}

const initialState: ConnectivityState = {
  browserOnline: true,
  lastHealthCheckOk: true,
};

export const connectivityFeature = createFeature({
  name: 'connectivity',
  reducer: createReducer(
    initialState,
    on(ConnectivityActions.browserReportedOnline, (s) => ({ ...s, browserOnline: true })),
    on(ConnectivityActions.browserReportedOffline, (s) => ({ ...s, browserOnline: false })),
    on(ConnectivityActions.healthCheckSucceeded, (s) => ({ ...s, lastHealthCheckOk: true })),
    on(ConnectivityActions.healthCheckFailed, (s) => ({ ...s, lastHealthCheckOk: false })),
  ),
  extraSelectors: ({ selectBrowserOnline, selectLastHealthCheckOk }) => ({
    // isOnline is true ONLY when both signals agree — either one reporting
    // offline is enough to mark the app offline.
    selectIsOnline: createSelector(
      selectBrowserOnline,
      selectLastHealthCheckOk,
      (browser, health) => browser && health,
    ),
  }),
});

export const {
  name: connectivityFeatureKey,
  reducer: connectivityReducer,
  selectIsOnline,
} = connectivityFeature;
