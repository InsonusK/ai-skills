import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { OrdersFacade, Order } from '@org/orders-data-access';

type Status = 'idle' | 'creating' | 'created' | 'error';
interface OrdersState {
  orders: Order[];
  status: Status;
  error: string | null;
}
const initial: OrdersState = { orders: [], status: 'idle', error: null };

export const OrdersStore = signalStore(
  { providedIn: 'root' },
  withState(initial),
  withComputed(({ orders }) => ({ count: computed(() => orders().length) })),
  withMethods((store, facade = inject(OrdersFacade)) => ({
    async load(): Promise<void> {
      patchState(store, { orders: await facade.list() });
    },
    async addOrder(product: string, quantity: number): Promise<void> {
      patchState(store, { status: 'creating', error: null });
      try {
        const order = await facade.addOrder({ product, quantity });
        patchState(store, { orders: [...store.orders(), order], status: 'created' });
      } catch (e) {
        patchState(store, { status: 'error', error: (e as Error).message });
      }
    },
  })),
);
