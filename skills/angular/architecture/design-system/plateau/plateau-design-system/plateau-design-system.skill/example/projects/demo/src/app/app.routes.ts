import { Routes } from '@angular/router';

// Preview components are authored next to each component in the library's
// spec/preview/ directory and imported here — never re-authored inside demo.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'button' },
  {
    path: 'button',
    loadComponent: () => import('@ds-preview/button').then((m) => m.DsButtonPreviewComponent),
  },
  {
    path: 'status-chip',
    loadComponent: () =>
      import('@ds-preview/status-chip').then((m) => m.DsStatusChipPreviewComponent),
  },
];
