import { inject, Injectable, InjectionToken } from '@angular/core';
import { LogEntry, LogLevel, LogSink } from './log-sink';
import { ConsoleLogSink } from './console-log-sink';
import { BackendLogSink } from './backend-log-sink';

export const LOG_SINKS = new InjectionToken<readonly LogSink[]>('LOG_SINKS', {
  providedIn: 'root',
  // VP6: BackendLogSink joins ConsoleLogSink on the same multi-sink seam —
  // no existing LoggerService call site changes.
  factory: () => [inject(ConsoleLogSink), inject(BackendLogSink)],
});

export const MIN_LOG_LEVEL = new InjectionToken<LogLevel>('MIN_LOG_LEVEL', {
  providedIn: 'root',
  factory: () => 'debug',
});

const ORDER: Record<LogLevel, number> = { debug: 0, info: 1, warn: 2, error: 3, report: 99 };

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
  /** Always sent to the backend, regardless of MIN_LOG_LEVEL. */
  report(m: string, c: Record<string, unknown> = {}) { this.log('report', m, c); }
  private log(level: LogLevel, message: string, c: Record<string, unknown>): void {
    // `report` bypasses level filtering by design.
    if (level !== 'report' && ORDER[level] < ORDER[this.min]) return;
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
  private root() { return new ScopedLogger(this.sinks, this.min, {}); }
  debug(m: string, c: Record<string, unknown> = {}) { this.root().debug(m, c); }
  info(m: string, c: Record<string, unknown> = {}) { this.root().info(m, c); }
  warn(m: string, c: Record<string, unknown> = {}) { this.root().warn(m, c); }
  error(m: string, c: Record<string, unknown> = {}) { this.root().error(m, c); }
  report(m: string, c: Record<string, unknown> = {}) { this.root().report(m, c); }
}
