import { TestBed } from '@angular/core/testing';
import { OrderDraftStore } from './orders-draft.store';

// the persist effect flushes on change detection — trigger it explicitly in tests
const settle = async () => {
  TestBed.tick();
  await Promise.resolve();
};

describe('OrderDraftStore (withPersistedDraft)', () => {
  beforeEach(() => localStorage.clear());
  afterEach(() => localStorage.clear());

  it('starts empty when storage has no draft', () => {
    const store = TestBed.inject(OrderDraftStore);
    expect(store.product()).toBe('');
    expect(store.quantity()).toBeNull();
  });

  it('rehydrates the draft from storage on construction', () => {
    localStorage.setItem('app:orders:draft', JSON.stringify({ product: 'Widget', quantity: 3 }));
    const store = TestBed.inject(OrderDraftStore);
    expect(store.product()).toBe('Widget');
    expect(store.quantity()).toBe(3);
  });

  it('persists the allow-listed keys on change', async () => {
    const store = TestBed.inject(OrderDraftStore);
    store.patch({ product: 'Gadget' });
    await settle();
    expect(JSON.parse(localStorage.getItem('app:orders:draft')!)).toEqual({
      product: 'Gadget',
      quantity: null,
    });
  });

  it('clear() empties the draft and storage', async () => {
    localStorage.setItem('app:orders:draft', JSON.stringify({ product: 'X', quantity: 1 }));
    const store = TestBed.inject(OrderDraftStore);
    store.clear();
    await settle();
    expect(store.product()).toBe('');
    expect(JSON.parse(localStorage.getItem('app:orders:draft')!)).toEqual({
      product: '',
      quantity: null,
    });
  });
});
