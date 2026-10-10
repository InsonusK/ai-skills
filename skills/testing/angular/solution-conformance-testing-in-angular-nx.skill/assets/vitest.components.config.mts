import { defineConfig } from "vitest/config";
import { resolve } from "node:path";

const kindDir = process.env["TEST_KIND_DIR"];
if (!kindDir) throw new Error("Run through make test-kind-components");

export default defineConfig({
  cacheDir: resolve(kindDir, "cache/vite"),
  test: {
    environment: "jsdom",
    passWithNoTests: false,
    allowOnly: false,
    retry: 0,
    coverage: {
      provider: "v8",
      reportsDirectory: resolve(kindDir, "report/components/coverage"),
      reporter: ["html", "json-summary"],
    },
  },
});
