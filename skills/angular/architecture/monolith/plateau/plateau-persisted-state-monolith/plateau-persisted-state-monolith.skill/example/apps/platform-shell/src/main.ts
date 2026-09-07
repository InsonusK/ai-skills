import { bootstrapApplication } from '@angular/platform-browser';
import { isDevMode } from '@angular/core';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig)
  .then(() => {
    // Register the Workbox-generated service worker only after bootstrap, and
    // never in dev (it fights live-reload / HMR).
    if (!isDevMode() && 'serviceWorker' in navigator) {
      navigator.serviceWorker
        .register('/sw.js')
        .catch((err) => console.error('SW registration failed', err));
    }
  })
  .catch((err) => console.error(err));
