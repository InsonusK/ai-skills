import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  { path: '', redirectTo: 'orders', pathMatch: 'full' },
  {
    path: 'orders',
    loadChildren: () => import('@org/orders-feature').then((m) => m.ORDERS_ROUTES),
  },
  { path: '**', redirectTo: 'orders' },
];
