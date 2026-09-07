import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { MutationQueueService, SyncStatus } from '@org/shared-offline-sync';
import { OrdersFacade, Order, AddOrderInput, isQueued } from '@org/orders-data-access';

/** An order row carries a per-entity sync status while it is not yet on the server. */
export type OrderRow = Order & { syncStatus?: SyncStatus };

interface OrdersState {
  orders: OrderRow[];
  submitting: boolean;
  submitError: string | null;
}
const initial: OrdersState = { orders: [], submitting: false, submitError: null };

const pendingId = (idempotencyKey: string) => `pending:${idempotencyKey}`;
const rowFrom = (payload: AddOrderInput, idempotencyKey: string): OrderRow => ({
  id: pendingId(idempotencyKey),
  product: payload.product,
  quantity: payload.quantity,
  createdAt: new Date(),
  syncStatus: 'queued',
});

export const OrdersStore = signalStore(
  { providedIn: 'root' },
  withState(initial),
  withComputed(({ orders }) => ({
    count: computed(() => orders().length),
    /** feeds <ui-pending-sync-indicator [count]> — everything not yet confirmed by the server */
    pendingSyncCount: computed(
      () => orders().filter((o) => o.syncStatus === 'queued' || o.syncStatus === 'sending' || o.syncStatus === 'failed').length,
    ),
  })),
  withMethods((store, facade = inject(OrdersFacade), queue = inject(MutationQueueService)) => ({
    async load(): Promise<void> {
      // keep the optimistic pending rows; replace the confirmed ones with the server list
      const pending = store.orders().filter((o) => o.syncStatus);
      patchState(store, { orders: [...(await facade.list()), ...pending] });
    },

    async addOrder(product: string, quantity: number): Promise<void> {
      patchState(store, { submitting: true, submitError: null });
      try {
        const result = await facade.addOrder({ product, quantity });
        const row: OrderRow = isQueued(result)
          ? { ...result.optimistic, syncStatus: 'queued' }
          : result;
        patchState(store, { orders: [...store.orders(), row], submitting: false });
      } catch (e) {
        patchState(store, { submitting: false, submitError: (e as Error).message });
      }
    },

    /** Driven by the FeatureReplay callbacks. `undefined` = synced → drop the optimistic row. */
    setSyncStatus(idempotencyKey: string, status: SyncStatus | undefined): void {
      const id = pendingId(idempotencyKey);
      patchState(store, {
        orders: status
          ? store.orders().map((o) => (o.id === id ? { ...o, syncStatus: status } : o))
          : store.orders().filter((o) => o.id !== id),
      });
    },

    /** Cold start: rebuild the optimistic rows from the persisted queue. */
    async hydratePending(): Promise<void> {
      const known = new Set(store.orders().map((o) => o.id));
      const rows = (await queue.pendingForFeatureOnce('orders'))
        .filter((e) => !known.has(pendingId(e.idempotencyKey)))
        .map((e) => rowFrom(e.payload as AddOrderInput, e.idempotencyKey));
      if (rows.length) patchState(store, { orders: [...store.orders(), ...rows] });
    },
  })),
);
