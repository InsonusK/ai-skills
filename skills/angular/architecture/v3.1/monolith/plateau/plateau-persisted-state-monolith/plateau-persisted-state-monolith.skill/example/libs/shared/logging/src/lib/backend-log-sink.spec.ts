import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { BaseHttpService } from '@org/shared-http-core';
import { BackendLogSink, MAX_BATCH_SIZE } from './backend-log-sink';
import { LogRetryQueue } from './log-retry-queue';
import { LogEntry } from './log-sink';

const entry = (level: LogEntry['level']): LogEntry => ({ level, message: 'm', context: {} });

describe('BackendLogSink', () => {
  const http = { post: vi.fn() };
  const retryQueue = {
    enqueue: vi.fn().mockResolvedValue(undefined),
    retryPending: vi.fn().mockResolvedValue(undefined),
  };
  let sink: BackendLogSink;

  beforeEach(() => {
    http.post.mockReset().mockReturnValue(of({}));
    retryQueue.enqueue.mockClear();
    retryQueue.retryPending.mockClear();
    TestBed.configureTestingModule({
      providers: [
        BackendLogSink,
        { provide: BaseHttpService, useValue: http },
        { provide: LogRetryQueue, useValue: retryQueue },
      ],
    });
    sink = TestBed.inject(BackendLogSink);
  });
  afterEach(() => sink.ngOnDestroy());

  it('buffers only warn/error/report — never debug/info', () => {
    sink.write(entry('debug'));
    sink.write(entry('info'));
    sink.write(entry('warn'));
    sink.write(entry('report'));
    expect((sink as unknown as { buffer: LogEntry[] }).buffer).toHaveLength(2);
  });

  it('flushes immediately once the buffer reaches MAX_BATCH_SIZE', async () => {
    for (let i = 0; i < MAX_BATCH_SIZE; i++) sink.write(entry('error'));
    await Promise.resolve();
    expect(http.post).toHaveBeenCalledWith('/logs', expect.any(Array));
  });

  it('hands a failed flush to the retry queue — never drops the batch', async () => {
    http.post.mockReturnValue(throwError(() => new Error('offline')));
    sink.write(entry('error'));
    await sink.flush();
    expect(retryQueue.enqueue).toHaveBeenCalledTimes(1);
  });

  it('opportunistically drains the retry queue on a successful flush', async () => {
    sink.write(entry('warn'));
    await sink.flush();
    expect(retryQueue.retryPending).toHaveBeenCalledTimes(1);
  });
});
