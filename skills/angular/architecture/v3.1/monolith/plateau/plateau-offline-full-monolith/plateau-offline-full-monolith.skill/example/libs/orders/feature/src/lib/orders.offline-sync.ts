import { inject, Provider } from '@angular/core';
import {
  FeatureReplay,
  PersistedMutation,
  provideFeatureReplay,
  ReplayConflictError,
} from '@org/shared-offline-sync';
import { AddOrderInput, OrdersConflictError, OrdersFacade } from '@org/orders-data-access';

/**
 * Register the `orders` replay handler at the composition root. Kept in the
 * feature lib so `shared-offline-sync` never imports a feature (no cycle).
 */
export function provideOrdersOfflineSync(): Provider {
  return provideFeatureReplay((): FeatureReplay => {
    const facade = inject(OrdersFacade);
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
    };
  });
}
