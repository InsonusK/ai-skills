import { DataTable, Given, When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { extractLinks } from "../../src/index";

interface World {
  text: string;
  links: string[];
}

Given("the text {string}", function (this: World, text: string) {
  this.text = text;
});

When("I extract the links", function (this: World) {
  this.links = extractLinks(this.text);
  console.log(`when: extractLinks(${JSON.stringify(this.text)}) -> ${JSON.stringify(this.links)}`);
});

Then("the links are:", function (this: World, table: DataTable) {
  assert.deepEqual(this.links, table.hashes().map((row) => row.link));
});

Then("the number of links is {int}", function (this: World, count: number) {
  assert.equal(this.links.length, count);
});
