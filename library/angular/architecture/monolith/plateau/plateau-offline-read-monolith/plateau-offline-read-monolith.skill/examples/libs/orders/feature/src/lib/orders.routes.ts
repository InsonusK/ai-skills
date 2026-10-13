import { Routes } from '@angular/router';
import { OrderFormComponent } from './order-form.component';

export const ORDERS_ROUTES: Routes = [
  // Main path: stays in the feature's own chunk.
  { path: '', component: OrderFormComponent },
  // Rarely visited and (in a real app) pulls in a heavy report/PDF dependency —
  // split into its own chunk with loadComponent so the form path never pays for it.
  {
    path: 'report',
    loadComponent: () =>
      import('./order-report/order-report.component').then((m) => m.OrderReportComponent),
  },
];
