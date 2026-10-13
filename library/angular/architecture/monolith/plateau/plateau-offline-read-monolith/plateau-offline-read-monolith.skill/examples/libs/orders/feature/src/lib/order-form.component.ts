import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StatusBadgeComponent } from '@org/shared-ui';
import { OrdersStore } from './orders.store';

@Component({
  selector: 'orders-order-form',
  standalone: true,
  imports: [FormsModule, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form (ngSubmit)="submit()">
      <label>Product <input name="product" [(ngModel)]="product" required /></label>
      <label>Quantity <input name="quantity" type="number" [(ngModel)]="quantity" required /></label>
      <button type="submit" [disabled]="store.status() === 'creating'">Add order</button>
    </form>
    <ui-status-badge [status]="store.status()" [label]="badgeLabel()" />
    @if (store.error(); as err) { <p role="alert">{{ err }}</p> }
    <ul>
      @for (o of store.orders(); track o.id) { <li>{{ o.product }} × {{ o.quantity }}</li> }
    </ul>
  `,
})
export class OrderFormComponent {
  readonly store = inject(OrdersStore);
  product = '';
  quantity: number | null = null;

  badgeLabel(): string {
    return { idle: 'Ready', creating: 'Creating…', created: 'Created', error: 'Failed' }[this.store.status()];
  }
  async submit(): Promise<void> {
    await this.store.addOrder(this.product, Number(this.quantity));
  }
}
