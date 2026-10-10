import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { BaseHttpService } from '@org/shared-http-core';
import { LOG_RETRY_QUEUE_LIMITS, LogRetryQueue, LogRetryQueueLimits } from './log-retry-queue';
import { LogEntry } from './log-sink';

const batch = (n = 1): LogEntry[] =>
  Array.from({ length: n }, () => ({ level: 'error' as const, message: 'm', context: {} }));

const DEFAULT_LIMITS: LogRetryQueueLimits = {
  maxEntryCount: 500,
  maxAgeMs: 7 * 24 * 3600_000,
  maxTotalBytes: 2 * 1024 * 1024,
};

describe('LogRetryQueue', () => {
  const http = { post: vi.fn() };
  let queue: LogRetryQueue;

  async function setup(limits: Partial<LogRetryQueueLimits> = {}): Promise<void> {
    http.post.mockReset().mockReturnValue(of({}));
    TestBed.configureTestingModule({
      providers: [
        LogRetryQueue,
        { provide: BaseHttpService, useValue: http },
        { provide: LOG_RETRY_QUEUE_LIMITS, useValue: { ...DEFAULT_LIMITS, ...limits } },
      ],
    });
    queue = TestBed.inject(LogRetryQueue);
    await queue.clear();
  }

  it('persists a batch and survives a "reload" (a fresh queue instance reads it)', async () => {
    await setup();
    await queue.enqueue(batch());
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        LogRetryQueue,
        { provide: BaseHttpService, useValue: http },
        { provide: LOG_RETRY_QUEUE_LIMITS, useValue: DEFAULT_LIMITS },
      ],
    });
    expect(await TestBed.inject(LogRetryQueue).count()).toBe(1);
  });

  it('evicts oldest-first once maxEntryCount is exceeded', async () => {
    await setup({ maxEntryCount: 2 });
    await queue.enqueue(batch());
    await queue.enqueue(batch());
    await queue.enqueue(batch()); // over the limit
    expect(await queue.count()).toBe(2);
  });

  it('retryPending sends and removes every batch when the network is available', async () => {
    await setup();
    await queue.enqueue(batch());
    await queue.enqueue(batch());
    await queue.retryPending();
    expect(http.post).toHaveBeenCalledTimes(2);
    expect(await queue.count()).toBe(0);
  });

  it('retryPending stops at the first failure — no burst of failing requests', async () => {
    await setup();
    await queue.enqueue(batch());
    await queue.enqueue(batch());
    http.post.mockReturnValue(throwError(() => new Error('still offline')));
    await queue.retryPending();
    expect(http.post).toHaveBeenCalledTimes(1);
    expect(await queue.count()).toBe(2);
  });
});
