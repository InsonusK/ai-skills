import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LoggerService } from '@org/shared-logging';
import { positive } from '@org/shared-util';
import { OrdersClient } from './orders.client';
import { AddOrderInput, Order } from './orders.model';
import { OrdersValidationError } from './orders.errors';

@Injectable({ providedIn: 'root' })
export class OrdersFacade {
  private readonly client = inject(OrdersClient);
  private readonly log = inject(LoggerService).forFeature('orders');

  list(): Promise<Order[]> {
    return firstValueFrom(this.client.list());
  }

  async addOrder(input: AddOrderInput): Promise<Order> {
    if (!positive(input.quantity)) {
      throw new OrdersValidationError('Quantity must be greater than zero');
    }
    const order = await firstValueFrom(this.client.add(input));
    this.log.info('Order created', { orderId: order.id });
    return order;
  }
}
