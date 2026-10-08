#!/usr/bin/env bash
# badges: tests coverage
# The unit test kind for .NET: builds the solution, runs every test project in one
# `dotnet test`, and normalizes the combined results into result/*.json, keeping a merged,
# browsable native report under report/. Coverage is collected and reported only in a
# report run.
set -euo pipefail
source tools/testing/kind.sh

# The one solution file in the repository root.
# (find, not a glob: under pipefail `ls *.slnx *.sln` fails when only one of the two exists.)
SOLUTION=$(find . -maxdepth 1 \( -name '*.slnx' -o -name '*.sln' \) -printf '%f\n' | sort | head -1)
[ -n "$SOLUTION" ] || { echo "no .slnx/.sln in $(pwd)" >&2; exit 2; }

WITH_CODE_COVERAGE=false
if [ "$TEST_RUN_PURPOSE" = report ]; then
  WITH_CODE_COVERAGE=true
  kind_mode "every test with coverage collected and reported"
else
  kind_mode "every test - coverage not collected"
fi

dotnet restore "$SOLUTION"
dotnet tool restore
dotnet build "$SOLUTION" --configuration Release --no-restore

TEST_RESULTS_DIR="$TEST_KIND_DIR/TestResults"

rm -rf "$TEST_RESULTS_DIR"
find . -name reqnroll_messages.ndjson -path "*/bin/*" -delete
mkdir -p "$RESULT_DIR" "$REPORT_DIR/tests"

COLLECT_ARGS=()
if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  COLLECT_ARGS=(--collect:"XPlat Code Coverage")
fi

# Running at the solution level picks up every test project in one command.
# LogFilePrefix gives each project's trx its own file name (a fixed LogFileName would
# make every project overwrite the same file), so we glob for every "test-results*.trx".
#
# verbosity=detailed prints every Feature/Scenario/step Reqnroll executed (with
# pass/fail), which is invaluable in CI logs on failure. @status/todo and @status/broken scenarios are excluded
# through Reqnroll's tag-to-trait mapping (Category=status/todo, Category=status/broken). The exit code is kept, not
# acted on yet, so the normalized results below are written on a red run too.
set +e
dotnet test "$SOLUTION" \
  --no-build --configuration Release \
  --filter "Category!=status/todo&Category!=status/broken" \
  --results-directory "$TEST_RESULTS_DIR" \
  --logger "console;verbosity=detailed" \
  --logger "trx;LogFilePrefix=test-results" \
  "${COLLECT_ARGS[@]}"
TEST_EXIT=$?
set -e

# Merge every test project's own Reqnroll html report into one browsable report/ folder.
# Each test project's reqnroll.json writes into its own bin/ folder, per the MUST rule
# in Repository.extend.md, avoiding path collisions between projects.
mkdir -p "$REPORT_DIR/tests"
while IFS= read -r -d '' report; do
  PROJECT_NAME=$(basename "$(dirname "$(dirname "$(dirname "$(dirname "$report")")")")")
  mkdir -p "$REPORT_DIR/tests/$PROJECT_NAME"
  cp "$report" "$REPORT_DIR/tests/$PROJECT_NAME/index.html"
done < <(find . -path "*/bin/Release/*/reqnroll_report.html" -print0)

TOTAL=0
PASSED=0
FAILED=0
while IFS= read -r -d '' trx; do
  COUNTERS=$(grep -o '<Counters[^/]*/>' "$trx")
  TOTAL=$((TOTAL + $(echo "$COUNTERS" | grep -oP 'total="\K[0-9]+')))
  PASSED=$((PASSED + $(echo "$COUNTERS" | grep -oP 'passed="\K[0-9]+')))
  FAILED=$((FAILED + $(echo "$COUNTERS" | grep -oP 'failed="\K[0-9]+')))
done < <(find "$TEST_RESULTS_DIR" -name 'test-results*.trx' -print0)
printf '{"total":%s,"passed":%s,"failed":%s}' "$TOTAL" "$PASSED" "$FAILED" > "$RESULT_DIR/unit-test.json"
kind_badge_count tests tests "$PASSED" "$TOTAL"

# Scenario report: each test project's Reqnroll "message" formatter output (Cucumber
# Messages) -> [{uri, line, status}], its project-relative uri made repo-relative.
SCENARIO_RESULTS="$(mktemp)"
trap 'rm -f "$SCENARIO_RESULTS"' EXIT
while IFS= read -r -d '' messages; do
  PROJECT_DIR=$(dirname "$(dirname "$(dirname "$(dirname "$messages")")")")
  PROJECT_DIR="${PROJECT_DIR#./}"
  jq -s --arg prefix "$PROJECT_DIR/" -f tools/testing/messages-results.jq "$messages"
done < <(find . -path "*/bin/Release/*/reqnroll_messages.ndjson" -print0) \
  | jq -s 'add // []' > "$SCENARIO_RESULTS"
bash tools/testing/normalize-scenarios.sh "$SCENARIO_RESULTS"
kind_scenarios_check || TEST_EXIT=1   # an untagged scenario or feature is a failed check

# Living doc: every project's Cucumber Messages file becomes the runner's standard report
# under report/tests/cucumber/, rendered by the shared tools/livingdoc.
mkdir -p "$REPORT_DIR/tests/cucumber"
while IFS= read -r -d '' messages; do
  PROJECT_NAME=$(basename "$(dirname "$(dirname "$(dirname "$(dirname "$messages")")")")")
  cp "$messages" "$REPORT_DIR/tests/cucumber/$PROJECT_NAME.ndjson"
done < <(find . -path "*/bin/Release/*/reqnroll_messages.ndjson" -print0)
kind_livingdoc

if [ "$WITH_CODE_COVERAGE" = "true" ]; then
  # The glob already matches every test project's own coverage.cobertura.xml, so
  # ReportGenerator merges them all into one summary without any extra wiring.
  dotnet reportgenerator \
    "-reports:$TEST_RESULTS_DIR/**/coverage.cobertura.xml" \
    "-targetdir:$REPORT_DIR/coverage" \
    -reporttypes:"Html;JsonSummary"

  LINE_PCT=$(jq '.summary.linecoverage' "$REPORT_DIR/coverage/Summary.json")
  rm "$REPORT_DIR/coverage/Summary.json"
  printf '{"linePct":%s}' "$LINE_PCT" > "$RESULT_DIR/coverage-test.json"
  kind_badge_percent coverage coverage "$LINE_PCT"
fi

exit "$TEST_EXIT"
