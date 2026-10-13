import { firstValueFrom } from 'rxjs';
import { MutationQueueService } from './mutation-queue.service';

describe('MutationQueueService', () => {
  let service: MutationQueueService;

  beforeEach(async () => {
    service = new MutationQueueService();
    await service.clear();
  });

  const entry = (feature: string, op = 'addOrder') => ({
    feature,
    operationName: op,
    payload: { product: 'W', quantity: 1 },
    touchedFields: ['product', 'quantity'],
  });

  it('enqueues with a generated idempotency key and an enqueue timestamp', async () => {
    const row = await service.enqueue(entry('orders'));
    expect(row.idempotencyKey).toMatch(/[0-9a-f-]{36}/);
    expect(row.enqueuedAt).toBeGreaterThan(0);
    expect(row.id).toBeTypeOf('number');
  });

  it('survives a full "page reload" — a new Dexie connection reads the same data', async () => {
    await service.enqueue(entry('orders'));
    const reopened = new MutationQueueService();
    const pending = await reopened.pendingForFeatureOnce('orders');
    expect(pending).toHaveLength(1);
  });

  it('partitions by feature and returns each partition FIFO', async () => {
    await service.enqueue(entry('orders', 'a'));
    await service.enqueue(entry('orders', 'b'));
    await service.enqueue(entry('billing', 'c'));
    const orders = await service.pendingForFeatureOnce('orders');
    expect(orders.map((e) => e.operationName)).toEqual(['a', 'b']);
    expect(await service.listFeatures()).toEqual(expect.arrayContaining(['orders', 'billing']));
  });

  it('markSynced removes the entry and the live view reflects it', async () => {
    const row = await service.enqueue(entry('orders'));
    await service.markSynced(row.id);
    const pending = await firstValueFrom(service.pendingForFeature$('orders'));
    expect(pending).toEqual([]);
  });
});
