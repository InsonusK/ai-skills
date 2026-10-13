export interface Order {
  readonly id: string;
  readonly product: string;
  readonly quantity: number;
  readonly createdAt: Date;
}
export interface AddOrderInput {
  readonly product: string;
  readonly quantity: number;
}
