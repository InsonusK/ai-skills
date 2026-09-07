import { firstValueFrom, of } from 'rxjs';
import { SelectivePreloadingStrategy } from './selective-preloading.strategy';

describe('SelectivePreloadingStrategy', () => {
  const strategy = new SelectivePreloadingStrategy();

  it('invokes load() when the route is flagged data.preload = true', async () => {
    let loaded = false;
    const load = () => {
      loaded = true;
      return of('chunk');
    };

    const result = await firstValueFrom(strategy.preload({ data: { preload: true } }, load));

    expect(loaded).toBe(true);
    expect(result).toBe('chunk');
  });

  it('does not invoke load() when the route has no preload flag', async () => {
    let loaded = false;
    const load = () => {
      loaded = true;
      return of('chunk');
    };

    const result = await firstValueFrom(strategy.preload({}, load));

    expect(loaded).toBe(false);
    expect(result).toBeNull();
  });

  it('does not invoke load() when data.preload is explicitly false', async () => {
    let loaded = false;
    const load = () => {
      loaded = true;
      return of('chunk');
    };

    await firstValueFrom(strategy.preload({ data: { preload: false } }, load));

    expect(loaded).toBe(false);
  });
});
