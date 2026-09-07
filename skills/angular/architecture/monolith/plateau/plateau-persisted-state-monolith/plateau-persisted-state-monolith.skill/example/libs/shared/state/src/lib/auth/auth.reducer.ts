import { createFeature, createReducer, createSelector, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { User } from './auth.model';

export interface AuthState {
  readonly currentUser: User | null;
  /** in-memory only — never persisted (token-storage-strategy ADR) */
  readonly accessToken: string | null;
  /** granular permission strings, never role names (authorization-model ADR) */
  readonly permissions: readonly string[];
  readonly refreshInProgress: boolean;
}

const initialState: AuthState = {
  currentUser: null,
  accessToken: null,
  permissions: [],
  refreshInProgress: false,
};

const loggedOut = (s: AuthState): AuthState => ({
  ...s,
  currentUser: null,
  accessToken: null,
  permissions: [],
  refreshInProgress: false,
});

export const authFeature = createFeature({
  name: 'auth',
  reducer: createReducer(
    initialState,
    on(AuthActions.loginSucceeded, (s, { user, accessToken, permissions }) => ({
      ...s,
      currentUser: user,
      accessToken,
      permissions,
    })),
    on(AuthActions.loginFailed, loggedOut),
    on(AuthActions.silentRefreshRequested, (s) => ({ ...s, refreshInProgress: true })),
    on(AuthActions.silentRefreshSucceeded, (s, { accessToken, permissions }) => ({
      ...s,
      accessToken,
      permissions,
      refreshInProgress: false,
    })),
    on(AuthActions.silentRefreshFailed, loggedOut),
    on(AuthActions.sessionExpired, loggedOut),
    on(AuthActions.logoutRequested, loggedOut),
  ),
  extraSelectors: ({ selectCurrentUser, selectAccessToken }) => ({
    selectIsLoggedIn: createSelector(selectCurrentUser, selectAccessToken, (u, t) => !!u && !!t),
  }),
});

export const {
  name: authFeatureKey,
  reducer: authReducer,
  selectCurrentUser,
  selectAccessToken,
  selectPermissions,
  selectIsLoggedIn,
} = authFeature;
