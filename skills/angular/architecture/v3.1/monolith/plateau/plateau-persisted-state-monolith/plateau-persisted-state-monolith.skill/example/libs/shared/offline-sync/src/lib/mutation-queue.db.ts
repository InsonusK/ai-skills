import Dexie, { Table } from 'dexie';

export interface QueuedMutation {
  id?: number;
  /** partition key — the feature (`scope`) that created the mutation */
  feature: string;
  /** generated once at enqueue time, reused across every replay attempt */
  idempotencyKey: string;
  /** the Facade method to replay, e.g. "addOrder" */
  operationName: string;
  /** the arguments the Facade method was called with */
  payload: unknown;
  /** fields this mutation intends to change — for server-wins conflict diffing.
   *  Derived from the command payload, never a separate entity snapshot. */
  touchedFields: string[];
  enqueuedAt: number;
}

export class MutationQueueDb extends Dexie {
  queuedMutations!: Table<QueuedMutation, number>;

  constructor(name = 'offline-sync') {
    super(name);
    this.version(1).stores({
      queuedMutations: '++id, feature, enqueuedAt',
    });
  }
}
