import { TestBed } from '@angular/core/testing';
import { LoggerService, LOG_SINKS, MIN_LOG_LEVEL } from './logger.service';
import { LogEntry } from './log-sink';

describe('LoggerService', () => {
  const written: LogEntry[] = [];
  const sink = { write: (e: LogEntry) => written.push(e) };

  beforeEach(() => {
    written.length = 0;
    TestBed.configureTestingModule({
      providers: [
        { provide: LOG_SINKS, useValue: [sink] },
        { provide: MIN_LOG_LEVEL, useValue: 'info' },
      ],
    });
  });

  it('forwards a structured entry to every sink, merging the feature context', () => {
    TestBed.inject(LoggerService).forFeature('orders').warn('boom', { orderId: '7' });
    expect(written).toHaveLength(1);
    expect(written[0]).toMatchObject({ level: 'warn', message: 'boom', context: { feature: 'orders', orderId: '7' } });
  });

  it('filters entries below MIN_LOG_LEVEL', () => {
    TestBed.inject(LoggerService).debug('noise');
    expect(written).toHaveLength(0);
  });

  it('always emits report() regardless of MIN_LOG_LEVEL (it is not a severity)', () => {
    TestBed.inject(LoggerService).report('deliberate event', { kind: 'signup' });
    expect(written).toHaveLength(1);
    expect(written[0].level).toBe('report');
  });
});
