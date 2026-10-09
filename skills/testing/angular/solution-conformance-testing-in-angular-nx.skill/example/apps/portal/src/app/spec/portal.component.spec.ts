import { TestBed } from "@angular/core/testing";
import { expect, it } from "vitest";
import { Portal } from "../app";
it("renders the application heading and the library form", async () => {
  const fixture = TestBed.createComponent(Portal);
  await fixture.whenStable();
  expect(fixture.nativeElement.querySelector("h1").textContent).toBe(
    "Review 1 link",
  );
  expect(
    fixture.nativeElement.querySelector("linkcheck-form input"),
  ).not.toBeNull();
});
