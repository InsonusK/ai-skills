import { createActionGroup, props } from '@ngrx/store';
import { Density, ThemeChoice } from './preferences.reducer';

export const PreferencesActions = createActionGroup({
  source: 'Preferences',
  events: {
    'Set Theme': props<{ theme: ThemeChoice }>(),
    'Set Density': props<{ density: Density }>(),
    'Remember Feature Tab': props<{ tab: string }>(),
  },
});
