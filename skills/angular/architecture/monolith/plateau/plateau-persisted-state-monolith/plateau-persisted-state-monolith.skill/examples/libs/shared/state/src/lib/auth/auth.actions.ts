import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { LoginCredentials, User } from './auth.model';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login Requested': props<{ credentials: LoginCredentials }>(),
    'Login Succeeded': props<{ user: User; accessToken: string; permissions: string[] }>(),
    'Login Failed': props<{ error: string }>(),
    'Silent Refresh Requested': emptyProps(),
    'Silent Refresh Succeeded': props<{ accessToken: string; permissions: string[] }>(),
    'Silent Refresh Failed': emptyProps(),
    'Session Expired': emptyProps(),
    'Logout Requested': emptyProps(),
  },
});
