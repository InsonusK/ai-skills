# scripts/normalize-scenarios.sh

Writes `$TEST_KIND_DIR/result/scenarios.json` per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|solution-conformance-testing's Scenario report]]. Stack-independent — the same script (unmodified) is used by the .NET and the TypeScript variants of this solution; it is duplicated verbatim in each rather than shared, so each solution stays self-contained. `scripts/unit-test.sh` calls it with the runner's per-scenario results already reduced to `[{"uri", "line", "status"}]`.

The inventory comes from scanning every `.feature` file (English Gherkin keywords), so `@todo` entries the runner never executes are listed too; a result is joined onto an entry by `uri` plus the `Scenario` line, or the `Examples:` row line for a `Scenario Outline` row.

Copy verbatim to `scripts/normalize-scenarios.sh` — the same file for every stack that uses scripts: [`assets/scripts/normalize-scenarios.sh`](../../../core/solution-conformance-testing.skill/assets/scripts/normalize-scenarios.sh)
