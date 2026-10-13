import { AuthActions } from './auth.actions';
import {
  authReducer,
  selectAccessToken,
  selectIsLoggedIn,
  selectPermissions,
} from './auth.reducer';

const initial = authReducer(undefined, { type: '@@init' });
const user = { id: 'u1', name: 'Ada' };

describe('auth slice', () => {
  it('loginSucceeded populates user, in-memory token and permissions', () => {
    const s = authReducer(
      initial,
      AuthActions.loginSucceeded({ user, accessToken: 'tok', permissions: ['orders.view'] }),
    );
    expect(s.currentUser).toEqual(user);
    expect(s.accessToken).toBe('tok');
    expect(s.permissions).toEqual(['orders.view']);
  });

  it('silentRefreshSucceeded refreshes the token without needing a re-login', () => {
    let s = authReducer(initial, AuthActions.loginSucceeded({ user, accessToken: 'old', permissions: [] }));
    s = authReducer(s, AuthActions.silentRefreshSucceeded({ accessToken: 'new', permissions: ['orders.view'] }));
    expect(s.accessToken).toBe('new');
    expect(s.currentUser).toEqual(user);
  });

  it.each(['sessionExpired', 'silentRefreshFailed', 'logoutRequested'] as const)(
    '%s clears currentUser, accessToken and permissions',
    (action) => {
      let s = authReducer(initial, AuthActions.loginSucceeded({ user, accessToken: 't', permissions: ['x'] }));
      s = authReducer(s, AuthActions[action]());
      expect(s).toMatchObject({ currentUser: null, accessToken: null, permissions: [] });
    },
  );

  it('selectors derive login state and never expose a role', () => {
    const state = { auth: authReducer(initial, AuthActions.loginSucceeded({ user, accessToken: 't', permissions: ['orders.delete'] })) };
    expect(selectIsLoggedIn(state)).toBe(true);
    expect(selectAccessToken(state)).toBe('t');
    expect(selectPermissions(state)).toEqual(['orders.delete']);
  });
});
