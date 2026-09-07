import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LoggerService } from '@org/shared-logging';
import { positive } from '@org/shared-util';
import { OfflineTransportError } from '@org/shared-http-core';
import { MutationQueueService } from '@org/shared-offline-sync';
import { OrdersClient } from './orders.client';
import { AddOrderInput, Order } from './orders.model';
import { OrdersValidationError } from './orders.errors';

/**
 * A queueable operation returns this instead of a result when it was queued.
 * `idempotencyKey` correlates the optimistic row the store shows with the
 * queue entry the orchestrator replays; `optimistic` is the row to display now.
 */
export interface QueuedResult {
  readonly queued: true;
  readonly idempotencyKey: string;
  readonly optimistic: Order;
}
export type AddOrderResult = Order | QueuedResult;

export function isQueued(r: AddOrderResult): r is QueuedResult {
  return (r as QueuedResult).queued === true;
}

@Injectable({ providedIn: 'root' })
export class OrdersFacade {
  private readonly client = inject(OrdersClient);
  private readonly queue = inject(MutationQueueService);
  private readonly log = inject(LoggerService).forFeature('orders');

  list(): Promise<Order[]> {
    return firstValueFrom(this.client.list());
  }

  async addOrder(input: AddOrderInput): Promise<AddOrderResult> {
    // Business validation always runs first — a validation failure is never queued.
    if (!positive(input.quantity)) {
      throw new OrdersValidationError('Quantity must be greater than zero');
    }
    try {
      const order = await firstValueFrom(this.client.add(input));
      this.log.info('Order created', { orderId: order.id });
      return order;
    } catch (error) {
      // addOrder is an explicitly queueable operation: on a network-level
      // failure, enqueue for later replay instead of surfacing an error.
      if (error instanceof OfflineTransportError) {
        const entry = await this.queue.enqueue({
          feature: 'orders',
          operationName: 'addOrder',
          payload: input,
          touchedFields: Object.keys(input),
        });
        this.log.info('Order queued for sync (offline)');
        return {
          queued: true,
          idempotencyKey: entry.idempotencyKey,
          optimistic: { id: `pending:${entry.idempotencyKey}`, ...input, createdAt: new Date() },
        };
      }
      throw error;
    }
  }

  /** Replay path — a plain client call; never re-enqueues. */
  replayAdd(input: AddOrderInput): Promise<Order> {
    return firstValueFrom(this.client.add(input));
  }
}
