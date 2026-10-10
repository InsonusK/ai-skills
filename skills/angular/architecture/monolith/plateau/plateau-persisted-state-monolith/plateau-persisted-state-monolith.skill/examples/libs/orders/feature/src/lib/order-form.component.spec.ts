import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { provideMockStore } from '@ngrx/store/testing';
import { MutationQueueService } from '@org/shared-offline-sync';
import { selectPermissions } from '@org/shared-state';
import { OrderFormComponent } from './order-form.component';
import { OrdersStore } from './orders.store';
import { OrdersFacade } from '@org/orders-data-access';

const fakeQueue = { pendingForFeatureOnce: vi.fn().mockResolvedValue([]) };
const providers = (facade: unknown) => [
  OrdersStore,
  { provide: OrdersFacade, useValue: facade },
  { provide: MutationQueueService, useValue: fakeQueue },
  provideMockStore({ selectors: [{ selector: selectPermissions, value: [] as string[] }] }),
];

describe('OrderFormComponent (behavioral — no business-layer mocks beyond the immediate collaborator)', () => {
  beforeEach(() => localStorage.clear());

  it('disables the submit button while a submit is in flight', async () => {
    const facade = { list: vi.fn().mockResolvedValue([]), addOrder: vi.fn(() => new Promise(() => undefined)) };
    await render(OrderFormComponent, { providers: providers(facade) });
    await userEvent.type(screen.getByRole('textbox', { name: /product/i }), 'Widget');
    await userEvent.type(screen.getByRole('spinbutton', { name: /quantity/i }), '2');
    await userEvent.click(screen.getByRole('button', { name: /add order/i }));
    expect(screen.getByRole('button', { name: /add order/i })).toBeDisabled();
  });

  it('shows the validation error returned by the store', async () => {
    const facade = {
      list: vi.fn().mockResolvedValue([]),
      addOrder: vi.fn().mockRejectedValue(new Error('Quantity must be greater than zero')),
    };
    await render(OrderFormComponent, { providers: providers(facade) });
    await userEvent.type(screen.getByRole('textbox', { name: /product/i }), 'Widget');
    await userEvent.type(screen.getByRole('spinbutton', { name: /quantity/i }), '0');
    await userEvent.click(screen.getByRole('button', { name: /add order/i }));
    expect(await screen.findByRole('alert')).toHaveTextContent(/greater than zero/i);
  });

  it('shows a "queued" badge on the new row when the facade queued the order offline', async () => {
    const facade = {
      list: vi.fn().mockResolvedValue([]),
      addOrder: vi.fn().mockResolvedValue({
        queued: true,
        idempotencyKey: 'k1',
        optimistic: { id: 'pending:k1', product: 'Widget', quantity: 2, createdAt: new Date() },
      }),
    };
    await render(OrderFormComponent, { providers: providers(facade) });
    await userEvent.type(screen.getByRole('textbox', { name: /product/i }), 'Widget');
    await userEvent.type(screen.getByRole('spinbutton', { name: /quantity/i }), '2');
    await userEvent.click(screen.getByRole('button', { name: /add order/i }));
    expect(await screen.findByText(/will sync/i)).toBeInTheDocument();
  });
});
