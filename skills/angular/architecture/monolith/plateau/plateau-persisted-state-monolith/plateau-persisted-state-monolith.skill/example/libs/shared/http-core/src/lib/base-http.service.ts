import { HttpClient } from '@angular/common/http';
import { inject, Injectable, InjectionToken } from '@angular/core';
import { Observable, retry, timer, timeout } from 'rxjs';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => '/api',
});

export interface RequestOptions {
  readonly timeoutMs?: number;
  /** Retry a transient failure this many times. Defaults to 1 for GET, 0 for non-idempotent verbs. */
  readonly retries?: number;
}

@Injectable({ providedIn: 'root' })
export class BaseHttpService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get<T>(path: string, opts: RequestOptions = {}): Observable<T> {
    return this.wrap(this.http.get<T>(this.url(path)), opts.timeoutMs, opts.retries ?? 1);
  }

  /** Non-idempotent by default — no retry (retrying a 409/duplicate write is unsafe). */
  post<T>(path: string, body: unknown, opts: RequestOptions = {}): Observable<T> {
    return this.wrap(this.http.post<T>(this.url(path), body), opts.timeoutMs, opts.retries ?? 0);
  }

  private url(path: string): string {
    return `${this.baseUrl}${path.startsWith('/') ? path : `/${path}`}`;
  }
  private wrap<T>(src: Observable<T>, timeoutMs = 15000, retries: number): Observable<T> {
    const withTimeout = src.pipe(timeout(timeoutMs));
    return retries > 0 ? withTimeout.pipe(retry({ count: retries, delay: () => timer(300) })) : withTimeout;
  }
}
