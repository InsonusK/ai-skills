# scripts/unit-test.sh

Runs `behave` and the plain `test/` suite under `coverage`, then normalizes the result into `tmp/result/*.json`, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]]. The JSON parsing below (behave's own `json.pretty` formatter, modeled after Cucumber's JSON schema) and the `coverage`/`jq` calls are solid; the HTML-formatter line is a choice you still have to pin — see the comment.

```bash
#!/usr/bin/env bash
# Runs the Cucumber/Gherkin conformance suite via behave (plus the plain test/ suite),
# both under coverage.py, and normalizes the results into tmp/result/*.json, keeping
# the native browsable report under tmp/report/.
#
# Params (env vars, optional):
#   WITH_CODE_COVERAGE=true   also collect and report line coverage
set -euo pipefail

WITH_CODE_COVERAGE="${WITH_CODE_COVERAGE:-false}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report"

rm -rf "$REPORT_DIR/tests" "$REPORT_DIR/coverage" .coverage
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"

BEHAVE_JSON="$(mktemp)"
trap 'rm -f "$BEHAVE_JSON"' EXIT

# behave has no built-in HTML formatter - pick one third-party plugin (e.g.
# behave-html-formatter, allure-behave) and pin it in pyproject.toml; verify its exact
# `--format`/`--outfile` invocation against the version you pin, this line is a sketch.
# The json.pretty line below is behave's own built-in formatter and is not a guess.
coverage run -m behave \
  --format progress \
  --format json.pretty --outfile "$BEHAVE_JSON"
# TODO: also run behave with the chosen HTML formatter (or convert $BEHAVE_JSON with
# a template) so tmp/report/tests/index.html exists before test-report.sh runs.

coverage run -a -m pytest test/

# behave's JSON formatter output is modeled after Cucumber's own JSON schema: a list of
# features, each with "elements" (scenarios), each with "steps" carrying a
# "result.status". Verify this against the behave version this project pins.
TOTAL=$(jq '[.[].elements[]] | length' "$BEHAVE_JSON")
PASSED=$(jq '[.[].elements[] | select(all(.steps[]; .result.status == "passed"))] | length' "$BEHAVE_JSON")
FAILED=$((TOTAL - PASSED))
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  coverage html -d "$REPORT_DIR/coverage"
  coverage json -o "$REPORT_DIR/coverage/coverage.json"
  LINE_PCT=$(jq '.totals.percent_covered' "$REPORT_DIR/coverage/coverage.json")
  rm "$REPORT_DIR/coverage/coverage.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
fi
```
