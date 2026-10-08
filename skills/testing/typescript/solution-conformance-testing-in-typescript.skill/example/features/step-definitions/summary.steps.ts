import { DataTable, Given, When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { summarizeBatch, type BatchSummary } from "../../src/index";

interface World {
  urls: string[];
  summary: BatchSummary;
}

Given("the URLs:", function (this: World, table: DataTable) {
  this.urls = table.hashes().map((row) => row.url);
});

Given("no URLs", function (this: World) {
  this.urls = [];
});

When("I summarize the batch", function (this: World) {
  this.summary = summarizeBatch(this.urls);
  console.log(`when: summarizeBatch(${JSON.stringify(this.urls)}) -> ${JSON.stringify(this.summary)}`);
});

Then("the batch has {int} valid and {int} invalid URLs", function (this: World, valid: number, invalid: number) {
  assert.deepEqual([this.summary.valid, this.summary.invalid], [valid, invalid]);
});

Then("the count of {string} errors is {int}", function (this: World, errorCode: string, count: number) {
  assert.equal(this.summary.errors[errorCode] ?? 0, count);
});
