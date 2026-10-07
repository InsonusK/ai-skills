# scripts/unit-test.sh

Runs every test project of the solution together — `@todo` scenarios excluded — and merges their Reqnroll scenarios, TRX counters, scenario results, and coverage (in a `report` run) into one normalized result, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. Writes `$TEST_KIND_DIR/result/unit-test.json` and `$TEST_KIND_DIR/result/scenarios.json` on a red run too, then exits with `dotnet test`'s own code. Verified with Reqnroll 3.3.4 + xUnit 2.9 on the VSTest runner, .NET 10 SDK.

Fill and copy to `scripts/unit-test.sh` — `{solution}` = the solution file name without `.slnx`: [`templates/scripts/unit-test.sh`](scripts/unit-test.sh)

## {TestProject}/reqnroll.json

Each test project has its own `reqnroll.json`. Both formatter paths are relative to that project's own `bin/` output, so parallel projects never overwrite each other's output. The `message` formatter (Cucumber Messages) is what the scenario report is built from:

Copy verbatim to every test project as `reqnroll.json`: [`assets/reqnroll.json`](../assets/reqnroll.json)
