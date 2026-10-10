import { Given, When, Then } from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import { writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { HistoryStore, StoreCorrupted } from '../store';
import { check } from '../checker';
Given('an empty history file', async function () {
  this.path = join(this.directory, 'history.jsonl');
  await writeFile(this.path, '');
  this.store = new HistoryStore(this.path);
  this.log('given: empty history');
});
Given('no history file', function () {
  this.store = new HistoryStore(join(this.directory, 'absent.jsonl'));
  this.log('given: absent history');
});
Given('a history file with the line {string}', async function (line: string) {
  this.path = join(this.directory, 'history.jsonl');
  await writeFile(this.path, line + '\n');
  this.store = new HistoryStore(this.path);
  this.log(`given: damaged line=${line}`);
});
When('I store the check of {string}', async function (url: string) {
  await this.store.append(check(url));
  this.log(`when: appended ${url}`);
});
When('I read the history', async function () {
  try {
    this.loaded = await this.store.load();
  } catch (error) {
    this.error = error;
  }
  this.log(`when: loaded=${JSON.stringify(this.loaded)} error=${this.error}`);
});
When(
  '{int} writers store {int} checks each at the same time',
  async function (writers: number, count: number) {
    await Promise.all(
      Array.from({ length: writers }, async () => {
        for (let number = 0; number < count; number++)
          await this.store.append(check(`https://example.com/${number}`));
      }),
    );
    this.log(`when: ${writers} writers x ${count}`);
  },
);
Then(/^the history holds (\d+) checks?$/, async function (count: string) {
  const loaded = await this.store.load();
  this.log(`then: count=${loaded.length}`);
  assert.equal(loaded.length, Number(count));
});
Then('the last stored URL is {string}', async function (url: string) {
  const loaded = await this.store.load();
  this.log(`then: last=${loaded.at(-1).normalized}`);
  assert.equal(loaded.at(-1).normalized, url);
});
Then('reading fails because the store is corrupted', function () {
  this.log(`then: error=${this.error}`);
  assert.ok(this.error instanceof StoreCorrupted);
});
