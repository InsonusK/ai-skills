export type LogLevel = 'debug' | 'info' | 'warn' | 'error';
export interface LogEntry {
  readonly level: LogLevel;
  readonly message: string;
  readonly context: Record<string, unknown>;
}
export interface LogSink {
  write(entry: LogEntry): void;
}
