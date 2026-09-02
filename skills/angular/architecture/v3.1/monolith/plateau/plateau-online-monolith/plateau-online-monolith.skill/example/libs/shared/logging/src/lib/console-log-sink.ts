import { Injectable } from '@angular/core';
import { LogEntry, LogSink } from './log-sink';

@Injectable({ providedIn: 'root' })
export class ConsoleLogSink implements LogSink {
  write(entry: LogEntry): void {
    // structured: one call, level-mapped
    const fn = entry.level === 'debug' ? 'debug' : entry.level;
    // eslint-disable-next-line no-console
    (console as unknown as Record<string, (...a: unknown[]) => void>)[fn](entry.message, entry.context);
  }
}
