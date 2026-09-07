import { ApplicationConfig, provideAppInitializer, inject, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { SESSION_CONTRACT } from '@platform/contracts';
import { routes } from './app.routes';
import { RemoteRegistryService } from './remote-registry/remote-registry.service';
import { HostSession } from './session/host-session';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // solution-session-sharing: the host is the ONLY provider of SESSION_CONTRACT.
    { provide: SESSION_CONTRACT, useExisting: HostSession },
    // solution-federation-host: load the runtime remote manifest at bootstrap.
    provideAppInitializer(() => inject(RemoteRegistryService).load().catch(() => void 0)),
  ],
};
