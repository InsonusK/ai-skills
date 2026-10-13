import { Injectable } from '@angular/core';
import { LogEntry, LogSink } from './log-sink';

@Injectable({ providedIn: 'root' })
export class ConsoleLogSink implements LogSink {
  write(entry: LogEntry): void {
    // The single sanctioned console call in the whole workspace — every other
    // part of the app logs through LoggerService, which forwards here.
    const method = entry.level === 'report' ? 'info' : entry.level;
    // eslint-disable-next-line no-console
    console[method](entry.message, entry.context);
  }
}
