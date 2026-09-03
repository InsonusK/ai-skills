export class OrdersError extends Error {}
export class OrdersConflictError extends OrdersError {
  constructor(readonly product: string) {
    super(`An order for "${product}" already exists`);
    this.name = 'OrdersConflictError';
  }
}
export class OrdersTransportError extends OrdersError {
  constructor(readonly status: number) {
    super(`Orders request failed (${status})`);
    this.name = 'OrdersTransportError';
  }
}
export class OrdersValidationError extends OrdersError {
  constructor(message: string) {
    super(message);
    this.name = 'OrdersValidationError';
  }
}
