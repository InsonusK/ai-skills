import { Given, When, Then, DataTable } from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import { summarize } from '../summary';
Given('the URLs:', function (table: DataTable) {
  this.urls = table.rows().map((row) => row[0]);
  this.log(`given: urls=${this.urls}`);
});
Given('no URLs', function () {
  this.urls = [];
  this.log('given: no URLs');
});
When('I summarize the batch', function () {
  this.summary = summarize(this.urls);
  this.log(`when: summary=${JSON.stringify(this.summary)}`);
});
Then('the batch has {int} valid and {int} invalid URLs', function (valid: number, invalid: number) {
  this.log(`then: valid=${this.summary.valid} invalid=${this.summary.invalid}`);
  assert.equal(this.summary.valid, valid);
  assert.equal(this.summary.invalid, invalid);
});
Then('the count of {string} errors is {int}', function (error: string, count: number) {
  this.log(`then: error count=${this.summary.errors[error]}`);
  assert.equal(this.summary.errors[error] ?? 0, count);
});
