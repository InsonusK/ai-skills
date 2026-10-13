import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withPreloading } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideGlobalStore } from '@org/shared-state';
import { provideOfflineSync } from '@org/shared-offline-sync';
import { appRoutes } from './app.routes';
import { SelectivePreloadingStrategy } from './preloading/selective-preloading.strategy';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes, withPreloading(SelectivePreloadingStrategy)),
    provideHttpClient(withFetch()),
    provideGlobalStore(),
    // VP5: the replay orchestrator (each feature registers its own replay
    // handler from its route providers — see ORDERS_ROUTES).
    provideOfflineSync(),
  ],
};
