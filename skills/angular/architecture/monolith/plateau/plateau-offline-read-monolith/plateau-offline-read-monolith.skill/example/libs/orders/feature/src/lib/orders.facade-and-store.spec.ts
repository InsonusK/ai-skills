import { TestBed } from '@angular/core/testing';
import { OrdersFacade } from '@org/orders-data-access';
import { OrdersStore } from './orders.store';

describe('OrdersStore (fakes the Facade — the layer directly beneath)', () => {
  const fakeFacade = {
    list: vi.fn().mockResolvedValue([]),
    addOrder: vi.fn(),
  };

  beforeEach(() => {
    fakeFacade.addOrder.mockReset();
    TestBed.configureTestingModule({
      providers: [{ provide: OrdersFacade, useValue: fakeFacade }],
    });
  });

  it('flips status creating -> created and appends the order on success', async () => {
    fakeFacade.addOrder.mockResolvedValue({ id: '1', product: 'W', quantity: 1, createdAt: new Date() });
    const store = TestBed.inject(OrdersStore);
    const done = store.addOrder('W', 1);
    // synchronously optimistic
    expect(store.status()).toBe('creating');
    await done;
    expect(store.status()).toBe('created');
    expect(store.count()).toBe(1);
  });

  it('flips status to error and records the message on a rejected Facade call', async () => {
    fakeFacade.addOrder.mockRejectedValue(new Error('Quantity must be greater than zero'));
    const store = TestBed.inject(OrdersStore);
    await store.addOrder('W', 0);
    expect(store.status()).toBe('error');
    expect(store.error()).toContain('greater than zero');
  });
});
