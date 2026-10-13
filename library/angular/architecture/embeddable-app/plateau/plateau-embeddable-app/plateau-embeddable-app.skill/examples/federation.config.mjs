import { withNativeFederation, shareAll } from '@angular-architects/native-federation/config';

export default withNativeFederation({
  name: 'embeddable-app',

  // The exposed module: this remote's own root-relative routes. The host mounts
  // them at one segment it chooses; this remote never knows which.
  exposes: {
    './Routes': './src/app/remote.routes.ts',
  },

  shared: {
    ...shareAll(
      { singleton: true, strictVersion: true, requiredVersion: 'auto', build: 'package' },
      {
        overrides: {
          '@angular/core': {
            singleton: true,
            strictVersion: true,
            requiredVersion: 'auto',
            build: 'package',
            includeSecondaries: { keepAll: true },
          },
          // The one build-time contract with the host — a strict singleton.
          '@platform/contracts': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
          // solution-remote-design-system-consumption: version-negotiated, NOT strict,
          // with THIS team's real tested range — a mismatch falls back to a bundled copy,
          // it never blocks this remote's own deploy.
          // 'design-system': { singleton: true, strictVersion: false, requiredVersion: '^0.1.0' },
        },
      },
    ),
  },

  skip: ['rxjs/ajax', 'rxjs/fetch', 'rxjs/testing', 'rxjs/webSocket'],

  features: { denseChunking: true },
});
