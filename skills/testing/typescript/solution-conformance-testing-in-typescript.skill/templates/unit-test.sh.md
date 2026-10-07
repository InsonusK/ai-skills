# scripts/unit-test.sh

Runs `cucumber-js` (`@todo` scenarios excluded; wrapped with `c8` for coverage in a `report` run), then normalizes the result into `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, on a red run too — and exits with `cucumber-js`'s own code, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. Verified with `@cucumber/cucumber` 13, `tsx` 4, TypeScript 7. `tsx` transpiles through esbuild, so it does not depend on the TypeScript compiler's version — unlike `ts-node`, which fails to load under TypeScript 6+.

Copy verbatim to `scripts/unit-test.sh`: [`assets/scripts/unit-test.sh`](../assets/scripts/unit-test.sh)
