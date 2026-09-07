import {
  ApplicationConfig,
  ErrorHandler,
  inject,
  provideAppInitializer,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter, withPreloading } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { Store } from '@ngrx/store';
import { AuthActions, provideGlobalStore } from '@org/shared-state';
import { authInterceptor } from '@org/shared-state';
import { provideOfflineSync } from '@org/shared-offline-sync';
import { appRoutes } from './app.routes';
import { SelectivePreloadingStrategy } from './preloading/selective-preloading.strategy';
import { GlobalErrorHandler } from './global-error-handler';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes, withPreloading(SelectivePreloadingStrategy)),
    // VP7: the auth interceptor attaches the in-memory token + triggers silent refresh on 401
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    provideGlobalStore(),
    // VP5: the replay orchestrator (each feature registers its own replay handler from its routes)
    provideOfflineSync(),
    // VP6: every uncaught exception → LoggerService.error → BackendLogSink
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    // VP7: exactly one silent-refresh attempt at bootstrap, before any authenticated request
    provideAppInitializer(() => {
      inject(Store).dispatch(AuthActions.silentRefreshRequested());
    }),
  ],
};
