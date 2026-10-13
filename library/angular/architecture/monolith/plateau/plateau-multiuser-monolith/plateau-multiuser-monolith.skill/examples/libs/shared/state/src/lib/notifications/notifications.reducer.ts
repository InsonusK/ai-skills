import { createFeature, createReducer, on } from '@ngrx/store';
import { NotificationsActions } from './notifications.actions';

export interface Notification {
  readonly id: string;
  readonly message: string;
  readonly detail?: Record<string, unknown>;
}

export interface NotificationsState {
  readonly items: readonly Notification[];
}

const initialState: NotificationsState = { items: [] };

export const notificationsFeature = createFeature({
  name: 'notifications',
  reducer: createReducer(
    initialState,
    on(NotificationsActions.show, (s, { message, detail }) => ({
      items: [...s.items, { id: crypto.randomUUID(), message, detail }],
    })),
    on(NotificationsActions.dismiss, (s, { id }) => ({
      items: s.items.filter((n) => n.id !== id),
    })),
    on(NotificationsActions.clearAll, () => initialState),
  ),
});

export const {
  name: notificationsFeatureKey,
  reducer: notificationsReducer,
  selectItems: selectNotifications,
} = notificationsFeature;
