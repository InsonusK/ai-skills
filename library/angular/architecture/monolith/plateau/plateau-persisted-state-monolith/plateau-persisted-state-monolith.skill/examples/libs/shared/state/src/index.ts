export * from './lib/store.config';
export { selectIsOnline } from './lib/connectivity/connectivity.selectors';
export {
  connectivityFeature,
  connectivityReducer,
  type ConnectivityState,
} from './lib/connectivity/connectivity.reducer';
export { ConnectivityActions } from './lib/connectivity/connectivity.actions';
export { selectNotifications } from './lib/notifications/notifications.selectors';
export {
  notificationsFeature,
  notificationsReducer,
  type Notification,
  type NotificationsState,
} from './lib/notifications/notifications.reducer';
export { NotificationsActions } from './lib/notifications/notifications.actions';
export {
  selectCurrentUser,
  selectAccessToken,
  selectPermissions,
  selectIsLoggedIn,
} from './lib/auth/auth.selectors';
export { authFeature, authReducer, type AuthState } from './lib/auth/auth.reducer';
export { AuthActions } from './lib/auth/auth.actions';
export { AuthFacade } from './lib/auth/auth.facade';
export { authInterceptor } from './lib/auth/auth.interceptor';
export type { User, LoginCredentials, RefreshResult } from './lib/auth/auth.model';

// VP8 — persisted state
export {
  persistKeys,
  assertPersistable,
  SENSITIVE_STATE_KEYS,
  type PersistConfig,
} from './lib/persistence/persisted-state';
export { withPersistedDraft } from './lib/persistence/with-persisted-draft';
export {
  selectTheme,
  selectDensity,
  selectLastFeatureTab,
} from './lib/preferences/preferences.selectors';
export {
  preferencesFeature,
  preferencesReducer,
  type PreferencesState,
  type ThemeChoice,
  type Density,
} from './lib/preferences/preferences.reducer';
export { PreferencesActions } from './lib/preferences/preferences.actions';

