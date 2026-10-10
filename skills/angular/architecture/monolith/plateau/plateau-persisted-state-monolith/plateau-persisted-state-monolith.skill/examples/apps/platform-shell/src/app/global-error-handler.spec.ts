import { TestBed } from '@angular/core/testing';
import { LoggerService } from '@org/shared-logging';
import { GlobalErrorHandler } from './global-error-handler';

describe('GlobalErrorHandler', () => {
  const logger = { error: vi.fn() };
  let handler: GlobalErrorHandler;

  beforeEach(() => {
    logger.error.mockReset();
    TestBed.configureTestingModule({
      providers: [GlobalErrorHandler, { provide: LoggerService, useValue: logger }],
    });
    handler = TestBed.inject(GlobalErrorHandler);
  });

  it('routes an uncaught Error through LoggerService.error with sanitized fields only', () => {
    const err = new Error('kaboom');
    handler.handleError(err);
    expect(logger.error).toHaveBeenCalledWith(
      'Uncaught exception',
      expect.objectContaining({ message: 'kaboom', stack: expect.any(String) }),
    );
    // never the raw error object
    expect(logger.error.mock.calls[0][1]).not.toHaveProperty('error');
  });

  it('handles a non-Error throw', () => {
    handler.handleError('a string');
    expect(logger.error).toHaveBeenCalledWith('Uncaught exception', { message: 'a string', stack: undefined });
  });
});
