import { test, expect } from "@playwright/test";
test("opens the portal with its domain heading and embedded library", async ({
  page,
}) => {
  await page.goto("/");
  await expect(
    page.getByRole("heading", { name: "Review 1 link" }),
  ).toBeVisible();
  await expect(page.getByLabel("URL", { exact: true })).toBeVisible();
});
