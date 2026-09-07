import { AddOrderInput, Order } from './orders.model';
import { OrderDto } from './orders.dto';

export const dtoToModel = (dto: OrderDto): Order => ({
  id: dto.id,
  product: dto.product_name,
  quantity: dto.qty,
  createdAt: new Date(dto.created_at),
});

export const inputToDto = (input: AddOrderInput): Pick<OrderDto, 'product_name' | 'qty'> => ({
  product_name: input.product,
  qty: input.quantity,
});
