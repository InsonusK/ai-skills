import {appendFile, readFile} from 'node:fs/promises';
import {Result} from './checker';
import {fromRecord, toRecord} from './mapping';
export class StoreCorrupted extends Error {}
export class HistoryStore {
  private pending: Promise<void> = Promise.resolve();
  constructor(private path: string) {}
  append(result: Result): Promise<void> {
    this.pending = this.pending.then(() => appendFile(this.path, JSON.stringify(toRecord(result)) + '\n', 'utf8'));
    return this.pending;
  }
  async load(): Promise<Result[]> {
    await this.pending;
    let content: string;
    try { content = await readFile(this.path, 'utf8'); }
    catch (error) { if ((error as NodeJS.ErrnoException).code === 'ENOENT') return []; throw error; }
    try { return content.split('\n').filter(Boolean).map(line => fromRecord(JSON.parse(line))); }
    catch (error) { throw new StoreCorrupted(String(error)); }
  }
}
