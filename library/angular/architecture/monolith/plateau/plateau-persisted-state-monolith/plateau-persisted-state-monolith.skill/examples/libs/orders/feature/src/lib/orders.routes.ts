import { Routes } from '@angular/router';
import { requirePermission } from '@org/shared-auth-ui';
import { OrderFormComponent } from './order-form.component';
import { provideOrdersOfflineSync } from './orders.offline-sync';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    // Route-level env injector: registers the feature's offline-sync replay
    // handler when the (lazy) feature loads — no feature code in the initial bundle.
    providers: [provideOrdersOfflineSync()],
    children: [
      // Main path: stays in the feature's own chunk.
      { path: '', component: OrderFormComponent },
      // Rarely visited and (in a real app) pulls in a heavy report/PDF dependency —
      // split into its own chunk with loadComponent so the form path never pays for it.
      {
        path: 'report',
        loadComponent: () =>
          import('./order-report/order-report.component').then((m) => m.OrderReportComponent),
      },
      // VP7: the guard is attached at the feature's OWN route (hierarchical
      // ownership) — never in the shell's app.routes.ts.
      {
        path: 'archive',
        canActivate: [requirePermission('orders.archive')],
        loadComponent: () =>
          import('./order-report/order-report.component').then((m) => m.OrderReportComponent),
      },
    ],
  },
];
