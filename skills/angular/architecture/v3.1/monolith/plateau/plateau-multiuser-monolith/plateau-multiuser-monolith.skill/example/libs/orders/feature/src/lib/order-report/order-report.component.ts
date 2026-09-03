import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { OrdersStore } from '../orders.store';

/**
 * A rarely-visited sub-page. In a real app this screen pulls in a heavy,
 * single-purpose dependency (a PDF renderer / a charting library) that the
 * order form never touches. It is split out of the feature's main chunk via
 * `loadComponent` in ORDERS_ROUTES so that weight is only fetched when a user
 * actually opens the report.
 */
@Component({
  selector: 'orders-order-report',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2>Order report</h2>
    <p>{{ count() }} order(s) recorded this session.</p>
    <table>
      <tbody>
        @for (o of store.orders(); track o.id) {
          <tr>
            <td>{{ o.product }}</td>
            <td>{{ o.quantity }}</td>
          </tr>
        }
      </tbody>
    </table>
  `,
})
export class OrderReportComponent {
  readonly store = inject(OrdersStore);
  readonly count = computed(() => this.store.orders().length);
}
