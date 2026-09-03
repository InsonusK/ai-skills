import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { OfflineTransportError } from '@org/shared-http-core';
import { MutationQueueService } from '@org/shared-offline-sync';
import { OrdersClient } from './orders.client';
import { OrdersFacade, isQueued } from './orders.facade';
import { OrdersValidationError } from './orders.errors';

describe('OrdersFacade (fakes the Client — the layer directly beneath)', () => {
  const client = { list: vi.fn(), add: vi.fn() };
  const queue = { enqueue: vi.fn().mockResolvedValue({}) };
  let facade: OrdersFacade;

  beforeEach(() => {
    client.add.mockReset();
    queue.enqueue.mockReset().mockResolvedValue({});
    TestBed.configureTestingModule({
      providers: [
        OrdersFacade,
        { provide: OrdersClient, useValue: client },
        { provide: MutationQueueService, useValue: queue },
      ],
    });
    facade = TestBed.inject(OrdersFacade);
  });

  it('returns the created order on success and does not enqueue', async () => {
    client.add.mockReturnValue(of({ id: '1', product: 'W', quantity: 2, createdAt: new Date() }));
    const result = await facade.addOrder({ product: 'W', quantity: 2 });
    expect(isQueued(result)).toBe(false);
    expect(queue.enqueue).not.toHaveBeenCalled();
  });

  it('enqueues and returns { queued: true } when the Client throws OfflineTransportError', async () => {
    client.add.mockReturnValue(throwError(() => new OfflineTransportError('add')));
    const result = await facade.addOrder({ product: 'W', quantity: 2 });
    expect(isQueued(result)).toBe(true);
    expect(queue.enqueue).toHaveBeenCalledWith(
      expect.objectContaining({ feature: 'orders', operationName: 'addOrder', touchedFields: ['product', 'quantity'] }),
    );
  });

  it('never enqueues an operation whose business validation already failed', async () => {
    await expect(facade.addOrder({ product: 'W', quantity: 0 })).rejects.toBeInstanceOf(OrdersValidationError);
    expect(client.add).not.toHaveBeenCalled();
    expect(queue.enqueue).not.toHaveBeenCalled();
  });

  it('re-throws a genuine server error (not OfflineTransportError) without enqueueing', async () => {
    client.add.mockReturnValue(throwError(() => new Error('500')));
    await expect(facade.addOrder({ product: 'W', quantity: 2 })).rejects.toThrow('500');
    expect(queue.enqueue).not.toHaveBeenCalled();
  });
});
