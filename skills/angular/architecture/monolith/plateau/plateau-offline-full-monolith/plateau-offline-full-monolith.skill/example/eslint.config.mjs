import nx from '@nx/eslint-plugin';

export default [
  ...nx.configs['flat/base'],
  ...nx.configs['flat/typescript'],
  ...nx.configs['flat/javascript'],
  {
    ignores: [
      '**/dist',
      '**/vite.config.*.timestamp*',
      '**/vitest.config.*.timestamp*',
    ],
  },
  {
    files: ['**/*.ts', '**/*.tsx', '**/*.js', '**/*.jsx'],
    rules: {
      // solution-logging-base: nothing outside libs/shared/logging's own
      // ConsoleLogSink may call console.* — everything logs through LoggerService.
      'no-console': 'error',
      '@nx/enforce-module-boundaries': [
        'error',
        {
          enforceBuildableLibDependency: true,
          allow: ['^.*/eslint(\\.base)?\\.config\\.[cm]?[jt]s$'],
          // solution-repository-structure: the two-axis (type / scope) allow-list.
          // Every project declares exactly one type:* and one scope:* tag.
          depConstraints: [
            // --- type axis (role) ---
            {
              sourceTag: 'type:app',
              onlyDependOnLibsWithTags: [
                'type:feature',
                'type:ui',
                'type:util',
                'type:store',
              ],
            },
            {
              sourceTag: 'type:e2e',
              onlyDependOnLibsWithTags: [],
            },
            {
              // A preview app composes real feature components with stubbed
              // data layers, so it may reference a feature's data-access tokens
              // (e.g. OrdersFacade) purely to provide test doubles.
              sourceTag: 'type:preview',
              onlyDependOnLibsWithTags: [
                'type:feature',
                'type:ui',
                'type:util',
                'type:store',
                'type:data-access',
              ],
            },
            {
              sourceTag: 'type:feature',
              onlyDependOnLibsWithTags: [
                'type:data-access',
                'type:ui',
                'type:util',
                'type:store',
              ],
            },
            {
              // + type:store — a feature Facade enqueues offline mutations via
              // libs/shared/offline-sync (VP5), which is a state-infra lib.
              sourceTag: 'type:data-access',
              onlyDependOnLibsWithTags: ['type:util', 'type:data-access', 'type:store'],
            },
            {
              // + type:store — libs/shared/offline-sync reads the connectivity /
              // notifications slices from libs/shared/state.
              sourceTag: 'type:store',
              onlyDependOnLibsWithTags: ['type:util', 'type:data-access', 'type:store'],
            },
            {
              sourceTag: 'type:ui',
              onlyDependOnLibsWithTags: ['type:ui', 'type:util'],
            },
            {
              sourceTag: 'type:util',
              onlyDependOnLibsWithTags: ['type:util'],
            },
            // --- scope axis (business area) ---
            // scope:platform is the composition root — no scope restriction here,
            // it reaches every feature; the type:app constraint above is enough.
            {
              sourceTag: 'scope:shared',
              onlyDependOnLibsWithTags: ['scope:shared'],
            },
            {
              sourceTag: 'scope:orders',
              onlyDependOnLibsWithTags: ['scope:orders', 'scope:shared'],
            },
          ],
        },
      ],
    },
  },
  {
    files: [
      '**/*.ts',
      '**/*.tsx',
      '**/*.cts',
      '**/*.mts',
      '**/*.js',
      '**/*.jsx',
      '**/*.cjs',
      '**/*.mjs',
    ],
    // Override or add rules here
    rules: {},
  },
  {
    // The bootstrap catch runs before Angular DI exists, so LoggerService is
    // not yet reachable — console is the only option there.
    files: ['**/src/main.ts'],
    rules: { 'no-console': 'off' },
  },
];
