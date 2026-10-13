import { createFeature, createReducer, on } from '@ngrx/store';
import { PreferencesActions } from './preferences.actions';

export type ThemeChoice = 'system' | 'light' | 'dark';
export type Density = 'comfortable' | 'compact';

export interface PreferencesState {
  readonly theme: ThemeChoice;
  readonly density: Density;
  readonly lastFeatureTab: string | null;
}

const initialState: PreferencesState = {
  theme: 'system',
  density: 'comfortable',
  lastFeatureTab: null,
};

export const preferencesFeature = createFeature({
  name: 'preferences',
  reducer: createReducer(
    initialState,
    on(PreferencesActions.setTheme, (s, { theme }) => ({ ...s, theme })),
    on(PreferencesActions.setDensity, (s, { density }) => ({ ...s, density })),
    on(PreferencesActions.rememberFeatureTab, (s, { tab }) => ({ ...s, lastFeatureTab: tab })),
  ),
});

export const {
  name: preferencesFeatureKey,
  reducer: preferencesReducer,
  selectTheme,
  selectDensity,
  selectLastFeatureTab,
} = preferencesFeature;
