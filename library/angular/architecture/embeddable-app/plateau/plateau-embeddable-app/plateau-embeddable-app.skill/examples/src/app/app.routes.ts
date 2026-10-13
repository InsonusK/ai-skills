import { Routes } from '@angular/router';
import { REMOTE_ROUTES } from './remote.routes';

// Standalone dev: mount our own exposed routes at the app root.
export const routes: Routes = [{ path: '', children: REMOTE_ROUTES }];
