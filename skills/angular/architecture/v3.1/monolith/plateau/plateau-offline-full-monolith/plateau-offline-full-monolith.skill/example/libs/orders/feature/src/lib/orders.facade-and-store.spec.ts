import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { MutationQueueService } from '@org/shared-offline-sync';
import { OrdersFacade } from '@org/orders-data-access';
import { OrdersStore } from './orders.store';

describe('OrdersStore (fakes the Facade — the layer directly beneath)', () => {
  const fakeFacade = {
    list: vi.fn().mockResolvedValue([]),
    addOrder: vi.fn(),
  };
  const fakeQueue = { pendingForFeature$: vi.fn().mockReturnValue(of([])), enqueue: vi.fn() };

  beforeEach(() => {
    fakeFacade.addOrder.mockReset();
    fakeQueue.pendingForFeature$.mockReturnValue(of([]));
    TestBed.configureTestingModule({
      providers: [
        { provide: OrdersFacade, useValue: fakeFacade },
        { provide: MutationQueueService, useValue: fakeQueue },
      ],
    });
  });

  it('flips status creating -> created and appends the order on success', async () => {
    fakeFacade.addOrder.mockResolvedValue({ id: '1', product: 'W', quantity: 1, createdAt: new Date() });
    const store = TestBed.inject(OrdersStore);
    const done = store.addOrder('W', 1);
    expect(store.status()).toBe('creating'); // synchronously optimistic
    await done;
    expect(store.status()).toBe('created');
    expect(store.count()).toBe(1);
  });

  it('flips status to "queued" (not error) when the Facade reports the mutation was queued offline', async () => {
    fakeFacade.addOrder.mockResolvedValue({ queued: true });
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 1);
    expect(store.status()).toBe('queued');
    expect(store.count()).toBe(0); // nothing appended — not created yet
  });

  it('flips status to error and records the message on a rejected Facade call', async () => {
    fakeFacade.addOrder.mockRejectedValue(new Error('Quantity must be greater than zero'));
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 0);
    expect(store.status()).toBe('error');
    expect(store.error()).toContain('greater than zero');
  });

  it('trackPendingSync mirrors the queue live view into pendingSync', () => {
    fakeQueue.pendingForFeature$.mockReturnValue(of([{}, {}]));
    const store = TestBed.inject(OrdersStore);
    store.trackPendingSync();
    expect(store.pendingSync()).toBe(2);
  });
});
