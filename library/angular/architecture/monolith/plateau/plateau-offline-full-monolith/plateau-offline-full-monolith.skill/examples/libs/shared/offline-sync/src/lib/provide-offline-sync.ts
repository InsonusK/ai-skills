import { ENVIRONMENT_INITIALIZER, EnvironmentProviders, inject, makeEnvironmentProviders, Provider } from '@angular/core';
import { FeatureReplay, MutationReplayRegistry, ReplayOrchestrator } from './replay-orchestrator';

/**
 * Register the replay orchestrator at the composition root, and instantiate it
 * eagerly so its connectivity effect is live from bootstrap.
 */
export function provideOfflineSync(): EnvironmentProviders {
  return makeEnvironmentProviders([
    ReplayOrchestrator,
    { provide: ENVIRONMENT_INITIALIZER, multi: true, useValue: () => inject(ReplayOrchestrator) },
  ]);
}

/**
 * A feature registers how to replay its own queued mutations — placed in the
 * feature's route `providers`, so it loads with the (lazy) feature and never
 * pulls feature code into the initial bundle. `factory` runs in an injection
 * context. Registering also kicks a replay in case the app is already online.
 */
export function provideFeatureReplay(factory: () => FeatureReplay): Provider {
  return {
    provide: ENVIRONMENT_INITIALIZER,
    multi: true,
    useValue: () => {
      inject(MutationReplayRegistry).register(factory());
      void inject(ReplayOrchestrator).replayAllPartitions();
    },
  };
}
