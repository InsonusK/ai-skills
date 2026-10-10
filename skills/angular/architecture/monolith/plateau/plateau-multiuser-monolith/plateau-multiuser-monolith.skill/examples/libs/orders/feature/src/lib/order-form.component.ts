import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PendingSyncIndicatorComponent, StatusBadgeComponent } from '@org/shared-ui';
import { HasPermissionDirective } from '@org/shared-auth-ui';
import { OrdersStore } from './orders.store';

@Component({
  selector: 'orders-order-form',
  standalone: true,
  imports: [FormsModule, StatusBadgeComponent, PendingSyncIndicatorComponent, HasPermissionDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form (ngSubmit)="submit()">
      <label>Product <input name="product" [(ngModel)]="product" required /></label>
      <label>Quantity <input name="quantity" type="number" [(ngModel)]="quantity" required /></label>
      <button type="submit" [disabled]="store.submitting()">Add order</button>
    </form>
    <ui-pending-sync-indicator [count]="store.pendingSyncCount()" />
    <!-- VP7: UI polish only — the backend still authorizes the action -->
    <button type="button" *hasPermission="'orders.delete'">Delete all orders</button>
    @if (store.submitError(); as err) { <p role="alert">{{ err }}</p> }
    <ul>
      @for (o of store.orders(); track o.id) {
        <li>
          {{ o.product }} × {{ o.quantity }}
          @if (o.syncStatus; as s) { <ui-status-badge [status]="s" [label]="badgeLabel(s)" /> }
        </li>
      }
    </ul>
  `,
})
export class OrderFormComponent implements OnInit {
  readonly store = inject(OrdersStore);
  product = '';
  quantity: number | null = null;

  ngOnInit(): void {
    // Cold start: rebuild optimistic rows from the persisted queue.
    void this.store.hydratePending();
  }

  badgeLabel(s: string): string {
    return (
      {
        queued: 'Queued — will sync',
        sending: 'Sending…',
        failed: 'Sync failed — will retry',
        conflict: "Not applied — changed elsewhere",
      }[s] ?? s
    );
  }

  async submit(): Promise<void> {
    await this.store.addOrder(this.product, Number(this.quantity));
  }
}
