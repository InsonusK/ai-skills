import { inject, Injectable, OnDestroy } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BaseHttpService } from '@org/shared-http-core';
import { LogEntry, LogSink } from './log-sink';
import { LogRetryQueue } from './log-retry-queue';

export const FLUSH_INTERVAL_MS = 10_000;
export const MAX_BATCH_SIZE = 20;

/**
 * The second LogSink. Only `warn` / `error` / `report` entries reach it; they
 * are buffered and flushed on a timer or size threshold. On the page-unload
 * path it uses `navigator.sendBeacon` (a normal request would be cancelled).
 * A failed flush is never dropped — the batch goes to `LogRetryQueue`.
 */
@Injectable({ providedIn: 'root' })
export class BackendLogSink implements LogSink, OnDestroy {
  private readonly http = inject(BaseHttpService);
  private readonly retryQueue = inject(LogRetryQueue);
  private buffer: LogEntry[] = [];
  private readonly timer = setInterval(() => void this.flush(), FLUSH_INTERVAL_MS);
  private readonly onUnload = () => this.flushViaBeacon();

  constructor() {
    if (typeof window !== 'undefined') window.addEventListener('pagehide', this.onUnload);
  }

  ngOnDestroy(): void {
    clearInterval(this.timer);
    if (typeof window !== 'undefined') window.removeEventListener('pagehide', this.onUnload);
  }

  write(entry: LogEntry): void {
    if (entry.level === 'warn' || entry.level === 'error' || entry.level === 'report') {
      this.buffer.push(entry);
      if (this.buffer.length >= MAX_BATCH_SIZE) void this.flush();
    }
  }

  async flush(): Promise<void> {
    if (this.buffer.length === 0) return;
    const batch = this.buffer;
    this.buffer = [];
    try {
      await firstValueFrom(this.http.post('/logs', batch));
      await this.retryQueue.retryPending(); // opportunistically drain older queued batches too
    } catch {
      await this.retryQueue.enqueue(batch); // never dropped
    }
  }

  private flushViaBeacon(): void {
    if (this.buffer.length === 0) return;
    navigator.sendBeacon('/api/logs', JSON.stringify(this.buffer));
    this.buffer = [];
  }
}
