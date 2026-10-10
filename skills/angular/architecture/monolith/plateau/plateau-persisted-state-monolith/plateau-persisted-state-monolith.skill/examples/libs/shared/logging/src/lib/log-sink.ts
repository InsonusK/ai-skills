/** `report` is not a severity — it marks an entry as "always send to the backend". */
export type LogLevel = 'debug' | 'info' | 'warn' | 'error' | 'report';
export interface LogEntry {
  readonly level: LogLevel;
  readonly message: string;
  readonly context: Record<string, unknown>;
}
export interface LogSink {
  write(entry: LogEntry): void;
}
