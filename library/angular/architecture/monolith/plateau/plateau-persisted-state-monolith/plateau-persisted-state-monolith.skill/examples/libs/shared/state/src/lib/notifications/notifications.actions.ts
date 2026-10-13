import { createActionGroup, emptyProps, props } from '@ngrx/store';

export const NotificationsActions = createActionGroup({
  source: 'Notifications',
  events: {
    Show: props<{ message: string; detail?: Record<string, unknown> }>(),
    Dismiss: props<{ id: string }>(),
    'Clear All': emptyProps(),
  },
});
