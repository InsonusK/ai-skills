/**
 * A request that never reached the server at all (network-level failure —
 * an `HttpErrorResponse` with `status === 0`). Defined once here so every
 * feature's Client throws the *same* type and callers can catch it uniformly.
 *
 * VP4 (solution-offline-first) only classifies the error. VP5
 * (solution-offline-sync) will catch this exact type to decide whether to
 * enqueue the failed mutation instead of surfacing it as a failure.
 */
export class OfflineTransportError extends Error {
  constructor(
    readonly operation: string,
    options?: { cause?: unknown },
  ) {
    super(`"${operation}" failed: the network is unreachable`, options);
    this.name = 'OfflineTransportError';
  }
}
