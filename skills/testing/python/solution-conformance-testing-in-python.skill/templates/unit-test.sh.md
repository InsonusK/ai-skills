# scripts/unit-test.sh

Runs `behave` (`@todo` scenarios excluded) and the plain `test/` suite under `coverage`, then normalizes the result into `$TEST_KIND_DIR/result/*.json` — `scenarios.json` included, on a red run too — and exits with the first failing runner's own code, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. The JSON parsing below (behave's own `json.pretty` formatter, modeled after Cucumber's JSON schema) and the `coverage`/`jq` calls are solid; the HTML-formatter line is a choice you still have to pin — see the comment. Verified with behave 1.3.3, coverage.py, pytest.

Copy verbatim to `scripts/unit-test.sh`: [`assets/scripts/unit-test.sh`](../assets/scripts/unit-test.sh)
