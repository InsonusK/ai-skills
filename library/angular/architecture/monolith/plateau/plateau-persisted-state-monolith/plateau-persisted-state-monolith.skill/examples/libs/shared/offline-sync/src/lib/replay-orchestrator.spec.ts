import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { selectIsOnline, NotificationsActions } from '@org/shared-state';
import { MutationQueueService } from './mutation-queue.service';
import {
  FeatureReplay,
  MutationReplayRegistry,
  ReplayConflictError,
  ReplayOrchestrator,
} from './replay-orchestrator';

const mutation = (feature: string, operationName: string) => ({
  feature,
  operationName,
  payload: {},
  touchedFields: ['priority'],
});

describe('ReplayOrchestrator', () => {
  let queue: MutationQueueService;
  let registry: MutationReplayRegistry;
  let store: MockStore;
  let dispatch: ReturnType<typeof vi.fn>;

  async function setup(replays: FeatureReplay[]): Promise<ReplayOrchestrator> {
    TestBed.configureTestingModule({
      providers: [
        provideMockStore({ selectors: [{ selector: selectIsOnline, value: true }] }),
        MutationQueueService,
        MutationReplayRegistry,
        ReplayOrchestrator,
      ],
    });
    dispatch = vi.fn();
    store = TestBed.inject(MockStore);
    (TestBed.inject(Store) as unknown as { dispatch: unknown }).dispatch = dispatch;
    queue = TestBed.inject(MutationQueueService);
    await queue.clear();
    registry = TestBed.inject(MutationReplayRegistry);
    replays.forEach((r) => registry.register(r));
    return TestBed.inject(ReplayOrchestrator);
  }

  it('replays a partition FIFO and clears each entry on success', async () => {
    const seen: string[] = [];
    const orch = await setup([
      { feature: 'orders', replay: async (e) => void seen.push(e.operationName) },
    ]);
    await queue.enqueue(mutation('orders', 'first'));
    await queue.enqueue(mutation('orders', 'second'));

    await orch.replayAllPartitions();

    expect(seen).toEqual(['first', 'second']);
    expect(await queue.pendingForFeatureOnce('orders')).toEqual([]);
  });

  it('a stuck partition never blocks another partition', async () => {
    const orch = await setup([
      { feature: 'flaky', replay: async () => { throw new Error('geo service down'); } },
      { feature: 'orders', replay: async () => undefined },
    ]);
    await queue.enqueue(mutation('flaky', 'x'));
    await queue.enqueue(mutation('orders', 'y'));

    await orch.replayAllPartitions();

    expect(await queue.pendingForFeatureOnce('flaky')).toHaveLength(1); // still queued
    expect(await queue.pendingForFeatureOnce('orders')).toHaveLength(0); // synced
  });

  it('skips a partition that has no registered handler yet', async () => {
    const orch = await setup([]); // nothing registered
    await queue.enqueue(mutation('orders', 'z'));
    await orch.replayAllPartitions();
    expect(await queue.pendingForFeatureOnce('orders')).toHaveLength(1);
  });

  it('on a replay conflict: server wins (entry dropped) + a field-scoped notification', async () => {
    const orch = await setup([
      { feature: 'orders', replay: async () => { throw new ReplayConflictError({ priority: 'high' }); } },
    ]);
    await queue.enqueue(mutation('orders', 'setPriority'));

    await orch.replayAllPartitions();

    expect(await queue.pendingForFeatureOnce('orders')).toEqual([]);
    expect(dispatch).toHaveBeenCalledWith(
      NotificationsActions.show({
        message: `Your change to priority in orders wasn't applied — it was changed elsewhere.`,
        detail: { priority: 'high' },
      }),
    );
    expect(store).toBeTruthy();
  });

  it('drives the per-entity sync lifecycle through onReplayStart / onReplayResult', async () => {
    const events: string[] = [];
    const track = (label: string): Partial<FeatureReplay> => ({
      onReplayStart: () => events.push(`${label}:sending`),
      onReplayResult: (_e, r) => events.push(`${label}:${r}`),
    });
    const orch = await setup([
      { feature: 'orders', replay: async () => undefined, ...track('ok') },
      { feature: 'flaky', replay: async () => { throw new Error('down'); }, ...track('bad') },
      {
        feature: 'clash',
        replay: async () => { throw new ReplayConflictError({ priority: 'x' }); },
        ...track('clash'),
      },
    ] as FeatureReplay[]);
    await queue.enqueue(mutation('orders', 'a'));
    await queue.enqueue(mutation('flaky', 'b'));
    await queue.enqueue(mutation('clash', 'c'));

    await orch.replayAllPartitions();

    expect(events).toEqual(
      expect.arrayContaining(['ok:sending', 'ok:synced', 'bad:sending', 'bad:failed', 'clash:sending', 'clash:conflict']),
    );
  });
});
