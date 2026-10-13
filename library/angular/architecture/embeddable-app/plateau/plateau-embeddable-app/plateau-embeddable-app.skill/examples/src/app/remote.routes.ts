import { Routes } from '@angular/router';
import { requirePermission } from './session/require-permission';

// The federation-exposed module. Root-relative paths only — the host assigns the
// mount segment, and this remote must not reference it.
export const REMOTE_ROUTES: Routes = [
  {
    path: '',
    canActivate: [requirePermission('reports.view')],
    loadComponent: () => import('./reports/reports.component').then((m) => m.ReportsComponent),
  },
];
