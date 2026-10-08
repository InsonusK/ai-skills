#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for TypeScript: StrykerJS over the whole package in a report run,
# where the score never fails the run; in a check run only over the source files changed
# since DELTA_BASE with the project's own threshold, and skipped without one or when none
# changed.
set -euo pipefail
source tools/testing/kind.sh

MUTATE_ARGS=()
if [ "$TEST_RUN_PURPOSE" = check ]; then
  [ -n "$DELTA_BASE" ] || kind_skip "a check run mutates only changed files and no DELTA_BASE was given"
  SINCE=$(git rev-parse --verify --quiet "$DELTA_BASE^{commit}") \
    || { echo "DELTA_BASE '$DELTA_BASE' does not name a commit" >&2; exit 2; }
  FILES=$(git diff --relative --name-only --diff-filter=ACMR "$SINCE" -- ':(glob)src/**/*.ts' | paste -sd, -)
  [ -n "$FILES" ] || kind_skip "no src/**/*.ts file changed since $DELTA_BASE"
  MUTATE_ARGS=(--mutate "$FILES")
  kind_mode "mutating only the files changed since $DELTA_BASE"
else
  kind_mode "mutating the whole package - the score never fails the run"
fi

npm install

MUTATION_DIR="$REPORT_DIR/mutation"
CONFIG_FILE="$(mktemp --suffix=.json)"
trap 'rm -f "$CONFIG_FILE"' EXIT

# A patched copy of stryker.conf.json: reports and the sandbox go below the kind directory,
# and the work directory is kept out of the sandbox. A report run overrides the break
# threshold to 0, so a low score never fails it; a check run keeps the project's own.
BREAK_FILTER='.'
[ "$TEST_RUN_PURPOSE" = check ] || BREAK_FILTER='.thresholds.break = 0'
jq --arg html "$MUTATION_DIR/reports/mutation-report.html" \
   --arg json "$MUTATION_DIR/reports/mutation-report.json" \
   --arg tmp "$TEST_KIND_DIR/stryker-tmp" \
   --arg work "$(realpath --relative-to=. "$TEST_WORK_DIR")" \
   "$BREAK_FILTER"'
    | .reporters = ["html", "json", "clear-text"]
    | .htmlReporter.fileName = $html
    | .jsonReporter.fileName = $json
    | .tempDirName = $tmp
    | .ignorePatterns = ((.ignorePatterns // []) + [$work])' \
  stryker.conf.json > "$CONFIG_FILE"

set +e
npx stryker run "$CONFIG_FILE" "${MUTATE_ARGS[@]}"
STRYKER_EXIT_CODE=$?
set -e

MUTATION_JSON="$MUTATION_DIR/reports/mutation-report.json"
if [ -f "$MUTATION_JSON" ]; then
  KILLED=$(jq '[.files[].mutants[].status] | map(select(. == "Killed")) | length' "$MUTATION_JSON")
  SURVIVED=$(jq '[.files[].mutants[].status] | map(select(. == "Survived")) | length' "$MUTATION_JSON")
  TIMEDOUT=$(jq '[.files[].mutants[].status] | map(select(. == "Timeout")) | length' "$MUTATION_JSON")
  NO_COVERAGE=$(jq '[.files[].mutants[].status] | map(select(. == "NoCoverage")) | length' "$MUTATION_JSON")
  TESTED=$((KILLED + SURVIVED + TIMEDOUT + NO_COVERAGE))
  if [ "$TESTED" -eq 0 ]; then
    SCORE="0.0"
  else
    SCORE=$(awk -v k="$KILLED" -v t="$TESTED" 'BEGIN { printf "%.1f", (k / t) * 100 }')
  fi
  printf '{"killed":%s,"survived":%s,"timedout":%s,"noCoverage":%s,"score":%s}' \
    "$KILLED" "$SURVIVED" "$TIMEDOUT" "$NO_COVERAGE" "$SCORE" > "$RESULT_DIR/mutation-test.json"
  kind_badge_percent mutation "mutation score" "$SCORE"
fi

# The normalized result is a side effect - the script's own exit code must still be
# Stryker's, so a real failure (or a broken threshold) fails the calling make target.
exit $STRYKER_EXIT_CODE
