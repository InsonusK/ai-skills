import { Given, When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { checkLink, type CheckResult } from "../../src/index";

interface World {
  url: string;
  result: CheckResult;
}

Given("the URL {string}", function (this: World, url: string) {
  this.url = url;
});

When("I check the URL", function (this: World) {
  this.result = checkLink(this.url);
  console.log(`when: checkLink(${JSON.stringify(this.url)}) -> ${JSON.stringify(this.result)}`);
});

Then("the check is valid", function (this: World) {
  assert.equal(this.result.isValid, true);
});

Then("the normalized URL is {string}", function (this: World, normalized: string) {
  assert.equal(this.result.normalized, normalized);
});

Then("the check is invalid with error {string}", function (this: World, errorCode: string) {
  assert.equal(this.result.isValid, false);
  assert.equal(this.result.errorCode, errorCode);
});
