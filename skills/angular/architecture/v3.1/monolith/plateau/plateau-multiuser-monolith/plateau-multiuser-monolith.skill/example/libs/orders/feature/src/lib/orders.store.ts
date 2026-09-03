import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap } from 'rxjs';
import { MutationQueueService } from '@org/shared-offline-sync';
import { OrdersFacade, Order, isQueued } from '@org/orders-data-access';

type Status = 'idle' | 'creating' | 'created' | 'queued' | 'error';
interface OrdersState {
  orders: Order[];
  status: Status;
  error: string | null;
  /** how many of this feature's mutations are queued and waiting to sync */
  pendingSync: number;
}
const initial: OrdersState = { orders: [], status: 'idle', error: null, pendingSync: 0 };

export const OrdersStore = signalStore(
  { providedIn: 'root' },
  withState(initial),
  withComputed(({ orders }) => ({ count: computed(() => orders().length) })),
  withMethods((store, facade = inject(OrdersFacade), queue = inject(MutationQueueService)) => ({
    async load(): Promise<void> {
      patchState(store, { orders: await facade.list() });
    },
    async addOrder(product: string, quantity: number): Promise<void> {
      patchState(store, { status: 'creating', error: null });
      try {
        const result = await facade.addOrder({ product, quantity });
        if (isQueued(result)) {
          patchState(store, { status: 'queued' });
        } else {
          patchState(store, { orders: [...store.orders(), result], status: 'created' });
        }
      } catch (e) {
        patchState(store, { status: 'error', error: (e as Error).message });
      }
    },
    /** keep `pendingSync` in step with the queue's live view for this feature */
    trackPendingSync: rxMethod<void>(
      pipe(
        switchMap(() => queue.pendingForFeature$('orders')),
        tap((entries) => patchState(store, { pendingSync: entries.length })),
      ),
    ),
  })),
);
