import { Injectable } from '@angular/core';
import { PreloadingStrategy, Route } from '@angular/router';
import { Observable, of } from 'rxjs';

/**
 * Preloads a lazily-loaded route's chunk only when the route that mounts it
 * carries `data: { preload: true }`. Everything else stays purely on-demand —
 * in particular, an embeddable module's federated remote chunk is never
 * warmed up unless the shell explicitly asked for it.
 *
 * The `preload` flag is set at the mounting point (the shell's `app.routes.ts`
 * for top-level segments), never inside a feature's own exported routes.
 */
@Injectable({ providedIn: 'root' })
export class SelectivePreloadingStrategy implements PreloadingStrategy {
  preload(route: Route, load: () => Observable<unknown>): Observable<unknown> {
    return route.data?.['preload'] === true ? load() : of(null);
  }
}
