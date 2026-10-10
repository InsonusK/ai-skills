import { When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { formatHeading } from "../index";
When("I format {int} links", function (count: number) {
  this.heading = formatHeading(count);
});
Then("the count heading is {string}", function (heading: string) {
  assert.equal(this.heading, heading);
});
When("I format a negative link count", function () {
  try {
    formatHeading(-1);
  } catch (error) {
    this.formatError = error;
  }
});
Then("the count is rejected", function () {
  assert.equal(this.formatError?.message, "invalid count");
});
