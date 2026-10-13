import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  { path: '', redirectTo: 'orders', pathMatch: 'full' },
  {
    path: 'orders',
    // The shell owns the preload decision: orders is on the critical path for
    // almost every session, so warm its chunk in the background after the first
    // navigation settles. The flag lives here, at the mounting point — never in
    // ORDERS_ROUTES itself.
    data: { preload: true },
    loadChildren: () => import('@org/orders-feature').then((m) => m.ORDERS_ROUTES),
  },
  // VP7: shared auth pages. `/orders`'s own permission-guarded routes live inside
  // ORDERS_ROUTES, not here.
  {
    path: 'login',
    loadComponent: () => import('@org/shared-auth-ui').then((m) => m.LoginFormComponent),
  },
  {
    path: 'forbidden',
    loadComponent: () => import('@org/shared-auth-ui').then((m) => m.ForbiddenPageComponent),
  },
  { path: '**', redirectTo: 'orders' },
];
