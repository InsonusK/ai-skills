import { withNativeFederation, shareAll } from '@angular-architects/native-federation/config';

export default withNativeFederation({
  name: 'platform-host',

  shared: {
    ...shareAll(
      { singleton: true, strictVersion: true, requiredVersion: 'auto', build: 'package' },
      {
        overrides: {
          // includeSecondaries is an opt-out of ignoreUnusedDeps, so all of
          // @angular/core is shared to prevent mismatches.
          '@angular/core': {
            singleton: true,
            strictVersion: true,
            requiredVersion: 'auto',
            build: 'package',
            includeSecondaries: { keepAll: true },
          },
          // solution-federation-host: the ONE build-time contract, a strict singleton.
          // An incompatible major on a remote is a visible load-time failure, never
          // a silently duplicated runtime.
          '@platform/contracts': { singleton: true, strictVersion: true, requiredVersion: 'auto' },
          // solution-host-design-system-consumption: version-negotiated, NOT strict —
          // a remote behind on its design-system version falls back to its own copy.
          // 'design-system': { singleton: true, strictVersion: false, requiredVersion: '^0.1.0' },
        },
      },
    ),
  },

  skip: [
    'rxjs/ajax',
    'rxjs/fetch',
    'rxjs/testing',
    'rxjs/webSocket',
    // Add further packages you don't need at runtime
  ],

  // Please read our FAQ about sharing libs:
  // https://shorturl.at/jmzH0

  features: {
    // ignoreUnusedDeps is enabled by default now
    // ignoreUnusedDeps: true,

    // Opt-in: groups chunks in remoteEntry.json for smaller metadata file
    denseChunking: true,
  },
});
