import { NotificationsActions } from './notifications.actions';
import { notificationsReducer, selectNotifications } from './notifications.reducer';

const empty = { items: [] as never[] };

describe('notifications slice', () => {
  it('appends a notification on show, with a generated id', () => {
    const s = notificationsReducer(empty, NotificationsActions.show({ message: 'hi', detail: { a: 1 } }));
    expect(s.items).toHaveLength(1);
    expect(s.items[0]).toMatchObject({ message: 'hi', detail: { a: 1 } });
    expect(s.items[0].id).toMatch(/[0-9a-f-]{36}/);
  });

  it('removes only the dismissed notification', () => {
    let s = notificationsReducer(empty, NotificationsActions.show({ message: 'a' }));
    s = notificationsReducer(s, NotificationsActions.show({ message: 'b' }));
    s = notificationsReducer(s, NotificationsActions.dismiss({ id: s.items[0].id }));
    expect(s.items.map((n) => n.message)).toEqual(['b']);
  });

  it('clearAll empties the slice', () => {
    let s = notificationsReducer(empty, NotificationsActions.show({ message: 'a' }));
    s = notificationsReducer(s, NotificationsActions.clearAll());
    expect(s.items).toEqual([]);
  });

  it('selectNotifications reads the items list', () => {
    const state = { notifications: { items: [{ id: '1', message: 'x' }] } };
    expect(selectNotifications(state)).toEqual([{ id: '1', message: 'x' }]);
  });
});
