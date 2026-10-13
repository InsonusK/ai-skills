import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { withPersistedDraft } from '@org/shared-state';

/** The in-progress "add order" form, preserved across a browser reload (VP8). */
export interface OrderDraftState {
  product: string;
  quantity: number | null;
}

const empty: OrderDraftState = { product: '', quantity: null };

export const OrderDraftStore = signalStore(
  { providedIn: 'root' },
  withState(empty),
  withPersistedDraft<OrderDraftState>({ key: 'app:orders:draft', keys: ['product', 'quantity'] }),
  withMethods((store) => ({
    patch(partial: Partial<OrderDraftState>): void {
      patchState(store, partial);
    },
    clear(): void {
      patchState(store, empty);
    },
  })),
);
