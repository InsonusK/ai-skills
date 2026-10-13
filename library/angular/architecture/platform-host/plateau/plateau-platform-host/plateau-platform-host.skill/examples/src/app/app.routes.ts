import { Routes } from '@angular/router';
import { RemoteRegistryService } from './remote-registry/remote-registry.service';
import { inject } from '@angular/core';

// The host mounts each remote at ONE root segment, resolved through the manifest —
// exactly like a local feature's loadChildren. No remote URL is compiled in.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'reports' },
  {
    path: 'reports',
    loadChildren: async () => {
      try {
        const m = (await inject(RemoteRegistryService).loadRemote('embeddable-app')) as {
          REMOTE_ROUTES: Routes;
        };
        return m.REMOTE_ROUTES;
      } catch {
        // A failed remote load degrades to a fallback slot — never a shell-wide crash.
        const { RemoteUnavailableComponent } = await import('./remote-unavailable.component');
        return [{ path: '', component: RemoteUnavailableComponent }] satisfies Routes;
      }
    },
  },
];
