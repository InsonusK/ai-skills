import { inject, Injectable, InjectionToken } from '@angular/core';
import { LogEntry, LogLevel, LogSink } from './log-sink';
import { ConsoleLogSink } from './console-log-sink';

export const LOG_SINKS = new InjectionToken<readonly LogSink[]>('LOG_SINKS', {
  providedIn: 'root',
  factory: () => [inject(ConsoleLogSink)],
});

export const MIN_LOG_LEVEL = new InjectionToken<LogLevel>('MIN_LOG_LEVEL', {
  providedIn: 'root',
  factory: () => 'debug',
});

const ORDER: Record<LogLevel, number> = { debug: 0, info: 1, warn: 2, error: 3 };

/** A logger bound to a fixed context. Returned by `LoggerService.forFeature`. */
export class ScopedLogger {
  constructor(
    private readonly sinks: readonly LogSink[],
    private readonly min: LogLevel,
    private readonly context: Record<string, unknown>,
  ) {}
  debug(m: string, c: Record<string, unknown> = {}) { this.log('debug', m, c); }
  info(m: string, c: Record<string, unknown> = {}) { this.log('info', m, c); }
  warn(m: string, c: Record<string, unknown> = {}) { this.log('warn', m, c); }
  error(m: string, c: Record<string, unknown> = {}) { this.log('error', m, c); }
  private log(level: LogLevel, message: string, c: Record<string, unknown>): void {
    if (ORDER[level] < ORDER[this.min]) return;
    const entry: LogEntry = { level, message, context: { ...this.context, ...c } };
    for (const s of this.sinks) s.write(entry);
  }
}

@Injectable({ providedIn: 'root' })
export class LoggerService {
  private readonly sinks = inject(LOG_SINKS);
  private readonly min = inject(MIN_LOG_LEVEL);

  forFeature(feature: string): ScopedLogger {
    return new ScopedLogger(this.sinks, this.min, { feature });
  }
  debug(m: string, c: Record<string, unknown> = {}) { new ScopedLogger(this.sinks, this.min, {}).debug(m, c); }
  info(m: string, c: Record<string, unknown> = {}) { new ScopedLogger(this.sinks, this.min, {}).info(m, c); }
  warn(m: string, c: Record<string, unknown> = {}) { new ScopedLogger(this.sinks, this.min, {}).warn(m, c); }
  error(m: string, c: Record<string, unknown> = {}) { new ScopedLogger(this.sinks, this.min, {}).error(m, c); }
}
