import { When, Then } from "@cucumber/cucumber";
import assert from "node:assert/strict";
import { portalHeading } from "../heading";
When("the portal reviews {int} link(s)", function (count: number) {
  this.portalHeading = portalHeading(count);
});
Then("the portal heading is {string}", function (heading: string) {
  assert.equal(this.portalHeading, heading);
});
