import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { OfflineTransportError } from '@org/shared-http-core';
import { OrdersClient } from './orders.client';
import { OrdersConflictError, OrdersTransportError } from './orders.errors';

describe('OrdersClient (transport contract — the only place HttpTestingController lives)', () => {
  let client: OrdersClient;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    client = TestBed.inject(OrdersClient);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('maps the DTO to the domain model on add', async () => {
    const promise = firstValue(client.add({ product: 'Widget', quantity: 2 }));
    const req = http.expectOne('/api/orders');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ product_name: 'Widget', qty: 2 });
    req.flush({ id: '1', product_name: 'Widget', qty: 2, created_at: '2026-01-01T00:00:00Z' });
    await expect(promise).resolves.toMatchObject({ id: '1', product: 'Widget', quantity: 2 });
  });

  it('turns a 409 into a typed OrdersConflictError — no raw HttpErrorResponse escapes', async () => {
    const promise = firstValue(client.add({ product: 'Widget', quantity: 1 }));
    http.expectOne('/api/orders').flush('conflict', { status: 409, statusText: 'Conflict' });
    await expect(promise).rejects.toBeInstanceOf(OrdersConflictError);
  });

  it('turns a network-level failure (status 0) into the shared OfflineTransportError', async () => {
    const promise = firstValue(client.add({ product: 'Widget', quantity: 1 }));
    http.expectOne('/api/orders').error(new ProgressEvent('error')); // no response — offline
    await expect(promise).rejects.toBeInstanceOf(OfflineTransportError);
  });

  it('still maps a real server error (500) to the feature domain error, not OfflineTransportError', async () => {
    const promise = firstValue(client.add({ product: 'Widget', quantity: 1 }));
    http.expectOne('/api/orders').flush('boom', { status: 500, statusText: 'Server Error' });
    await expect(promise).rejects.toBeInstanceOf(OrdersTransportError);
  });
});

function firstValue<T>(obs: import('rxjs').Observable<T>): Promise<T> {
  return new Promise((res, rej) => obs.subscribe({ next: res, error: rej }));
}
