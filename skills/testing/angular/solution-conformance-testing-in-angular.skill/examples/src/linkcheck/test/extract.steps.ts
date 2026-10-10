import { Given, When, Then, DataTable } from '@cucumber/cucumber';
import assert from 'node:assert/strict';
import { extractLinks } from '../extractor';
Given('the text {string}', function (text: string) {
  this.text = text;
  this.log(`given: text=${text}`);
});
When('I extract the links', function () {
  this.links = extractLinks(this.text);
  this.log(`when: links=${JSON.stringify(this.links)}`);
});
Then('the links are:', function (table: DataTable) {
  this.log(`then: links=${JSON.stringify(this.links)}`);
  assert.deepEqual(
    this.links,
    table.rows().map((row) => row[0]),
  );
});
Then('the number of links is {int}', function (count: number) {
  this.log(`then: count=${this.links.length}`);
  assert.equal(this.links.length, count);
});
