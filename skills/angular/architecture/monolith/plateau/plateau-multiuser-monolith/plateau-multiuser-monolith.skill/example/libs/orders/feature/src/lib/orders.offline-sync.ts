import { inject, Provider } from '@angular/core';
import {
  FeatureReplay,
  PersistedMutation,
  provideFeatureReplay,
  ReplayConflictError,
} from '@org/shared-offline-sync';
import { AddOrderInput, OrdersConflictError, OrdersFacade } from '@org/orders-data-access';
import { OrdersStore } from './orders.store';

/**
 * Register the `orders` replay handler at the composition root. Kept in the
 * feature lib so `shared-offline-sync` never imports a feature (no cycle).
 * The `onReplay*` callbacks drive the per-order `syncStatus` in the feature store.
 */
export function provideOrdersOfflineSync(): Provider {
  return provideFeatureReplay((): FeatureReplay => {
    const facade = inject(OrdersFacade);
    const store = inject(OrdersStore);
    return {
      feature: 'orders',
      async replay(entry: PersistedMutation): Promise<void> {
        if (entry.operationName !== 'addOrder') return;
        try {
          await facade.replayAdd(entry.payload as AddOrderInput);
        } catch (error) {
          if (error instanceof OrdersConflictError) {
            // server-wins: report only the field this mutation touched
            throw new ReplayConflictError({ product: error.product });
          }
          throw error;
        }
      },
      onReplayStart: (entry) => store.setSyncStatus(entry.idempotencyKey, 'sending'),
      onReplayResult: (entry, result) =>
        store.setSyncStatus(entry.idempotencyKey, result === 'synced' ? undefined : result),
    };
  });
}
