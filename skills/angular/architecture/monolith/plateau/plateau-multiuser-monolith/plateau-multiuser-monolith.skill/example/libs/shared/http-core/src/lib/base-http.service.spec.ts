import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BaseHttpService } from './base-http.service';

describe('BaseHttpService', () => {
  let svc: BaseHttpService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    svc = TestBed.inject(BaseHttpService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('prefixes the base URL', () => {
    svc.get('/things').subscribe();
    http.expectOne('/api/things').flush([]);
  });

  it('retries a transient GET failure once, then succeeds', async () => {
    const p = new Promise<unknown>((res, rej) => svc.get('/things').subscribe({ next: res, error: rej }));
    http.expectOne('/api/things').flush('boom', { status: 503, statusText: 'Unavailable' });
    await new Promise((r) => setTimeout(r, 350));
    http.expectOne('/api/things').flush([{ ok: true }]);
    await expect(p).resolves.toEqual([{ ok: true }]);
  });

  it('never retries a POST — a duplicate write is unsafe', async () => {
    const p = new Promise((_, rej) => svc.post('/things', {}).subscribe({ error: rej }));
    http.expectOne('/api/things').flush('nope', { status: 409, statusText: 'Conflict' });
    await expect(p).rejects.toMatchObject({ status: 409 });
    http.expectNone('/api/things');
  });
});
