import { ErrorHandler, inject, Injectable } from '@angular/core';
import { LoggerService } from '@org/shared-logging';

/**
 * Routes every uncaught exception through `LoggerService.error` — from there it
 * reaches `BackendLogSink` like any other error log. Only sanitized fields
 * (`message`, `stack`) are extracted; the raw error object is never logged.
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly logger = inject(LoggerService);

  handleError(error: unknown): void {
    this.logger.error('Uncaught exception', {
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined,
    });
  }
}
