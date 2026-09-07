/**
 * Typed host<->remote event channels. Draft — only SessionContract is worked out
 * today; this interface is the placeholder the platform will fill in.
 */
export interface EventBus {
  publish<T>(channel: string, payload: T): void;
  subscribe<T>(channel: string, handler: (payload: T) => void): () => void;
}
