import { PreferencesActions } from './preferences.actions';
import {
  preferencesReducer,
  selectDensity,
  selectLastFeatureTab,
  selectTheme,
} from './preferences.reducer';

const initial = { theme: 'system' as const, density: 'comfortable' as const, lastFeatureTab: null };

describe('preferences slice', () => {
  it('setTheme replaces the theme', () => {
    const s = preferencesReducer(initial, PreferencesActions.setTheme({ theme: 'dark' }));
    expect(s.theme).toBe('dark');
  });

  it('setDensity replaces the density', () => {
    const s = preferencesReducer(initial, PreferencesActions.setDensity({ density: 'compact' }));
    expect(s.density).toBe('compact');
  });

  it('rememberFeatureTab records the last tab', () => {
    const s = preferencesReducer(initial, PreferencesActions.rememberFeatureTab({ tab: 'archive' }));
    expect(s.lastFeatureTab).toBe('archive');
  });

  it('selectors read a mounted preferences slice', () => {
    const state = { preferences: { theme: 'light', density: 'compact', lastFeatureTab: 'x' } };
    expect(selectTheme(state)).toBe('light');
    expect(selectDensity(state)).toBe('compact');
    expect(selectLastFeatureTab(state)).toBe('x');
  });
});
