import { TestBed } from '@angular/core/testing';
import { MutationQueueService } from '@org/shared-offline-sync';
import { OrdersFacade } from '@org/orders-data-access';
import { OrdersStore } from './orders.store';

describe('OrdersStore (fakes the Facade — the layer directly beneath)', () => {
  const fakeFacade = { list: vi.fn().mockResolvedValue([]), addOrder: vi.fn() };
  const fakeQueue = { pendingForFeatureOnce: vi.fn().mockResolvedValue([]) };

  beforeEach(() => {
    fakeFacade.addOrder.mockReset();
    fakeFacade.list.mockResolvedValue([]);
    fakeQueue.pendingForFeatureOnce.mockResolvedValue([]);
    TestBed.configureTestingModule({
      providers: [
        { provide: OrdersFacade, useValue: fakeFacade },
        { provide: MutationQueueService, useValue: fakeQueue },
      ],
    });
  });

  const queued = () => ({
    queued: true,
    idempotencyKey: 'k1',
    optimistic: { id: 'pending:k1', product: 'W', quantity: 1, createdAt: new Date() },
  });

  it('appends a plain row (no syncStatus) on an online success', async () => {
    fakeFacade.addOrder.mockResolvedValue({ id: '1', product: 'W', quantity: 1, createdAt: new Date() });
    const store = TestBed.inject(OrdersStore);
    const done = store.addOrder('W', 1);
    expect(store.submitting()).toBe(true); // synchronously optimistic
    await done;
    expect(store.submitting()).toBe(false);
    expect(store.count()).toBe(1);
    expect(store.orders()[0].syncStatus).toBeUndefined();
    expect(store.pendingSyncCount()).toBe(0);
  });

  it('appends an optimistic row with syncStatus "queued" when the Facade queued it offline', async () => {
    fakeFacade.addOrder.mockResolvedValue(queued());
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 1);
    expect(store.orders()[0]).toMatchObject({ id: 'pending:k1', syncStatus: 'queued' });
    expect(store.pendingSyncCount()).toBe(1);
  });

  it('records the message on a rejected Facade call, without a row', async () => {
    fakeFacade.addOrder.mockRejectedValue(new Error('Quantity must be greater than zero'));
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 0);
    expect(store.submitError()).toContain('greater than zero');
    expect(store.count()).toBe(0);
  });

  it('setSyncStatus drives the per-row status: queued -> sending -> synced (row dropped)', async () => {
    fakeFacade.addOrder.mockResolvedValue(queued());
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 1);

    store.setSyncStatus('k1', 'sending');
    expect(store.orders()[0].syncStatus).toBe('sending');
    expect(store.pendingSyncCount()).toBe(1);

    store.setSyncStatus('k1', undefined); // synced
    expect(store.count()).toBe(0);
    expect(store.pendingSyncCount()).toBe(0);
  });

  it('a conflict marks the row and stops counting it as pending', async () => {
    fakeFacade.addOrder.mockResolvedValue(queued());
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 1);
    store.setSyncStatus('k1', 'conflict');
    expect(store.orders()[0].syncStatus).toBe('conflict');
    expect(store.pendingSyncCount()).toBe(0);
  });

  it('hydratePending rebuilds the optimistic rows from the persisted queue on a cold start', async () => {
    fakeQueue.pendingForFeatureOnce.mockResolvedValue([
      {
        id: 7,
        idempotencyKey: 'k9',
        operationName: 'addOrder',
        payload: { product: 'W', quantity: 4 },
        touchedFields: ['product', 'quantity'],
        enqueuedAt: 1,
      },
    ]);
    const store = TestBed.inject(OrdersStore);
    await store.hydratePending();
    expect(store.orders()).toHaveLength(1);
    expect(store.orders()[0]).toMatchObject({ id: 'pending:k9', product: 'W', quantity: 4, syncStatus: 'queued' });
  });
});
