#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for TypeScript: StrykerJS over the whole package in a report run, where the
# score never fails the run; in a check run only over source files changed since DELTA_BASE
# with the real threshold, and skipped without one.
set -euo pipefail
source tools/testing/kind.sh

if [ "$TEST_RUN_PURPOSE" = check ]; then
  [ -n "$DELTA_BASE" ] || kind_skip "a check run mutates only changed files and no DELTA_BASE was given"
  kind_mode "mutating only files changed since $DELTA_BASE"
else
  kind_mode "mutating the whole package - the score never fails the run"
fi

npm install

MUTATION_DIR="$REPORT_DIR/mutation"
rm -rf "$MUTATION_DIR"

MUTATE_ARGS=()
if [ "$TEST_RUN_PURPOSE" = check ]; then
  FILES=$(git diff --name-only --diff-filter=ACMR "$DELTA_BASE" HEAD -- 'src/**/*.ts' | paste -sd, -)
  if [ -z "$FILES" ]; then
    kind_skip "no changes in src/**/*.ts since $DELTA_BASE"
  fi
  MUTATE_ARGS=(--mutate "$FILES")
fi

CONFIG_FILE="$(mktemp --suffix=.json)"
trap 'rm -f "$CONFIG_FILE"' EXIT

if [ "$TEST_RUN_PURPOSE" = check ]; then
  # Real threshold from stryker.conf.json applies here - the score has to be good
  # enough to pass the PR gate.
  jq --arg html "$MUTATION_DIR/reports/mutation-report.html" \
     --arg json "$MUTATION_DIR/reports/mutation-report.json" \
     '.reporters = ["html", "json", "clear-text"]
      | .htmlReporter.fileName = $html
      | .jsonReporter.fileName = $json' \
    stryker.conf.json > "$CONFIG_FILE"
else
  # Full run has no PR base to diff against, so the whole package is mutated; the break
  # threshold is overridden to 0 so a low score never fails this run - it only reports
  # the score, it doesn't gate anything. A check run enforces the real
  # threshold from stryker.conf.json before code reaches master.
  jq --arg html "$MUTATION_DIR/reports/mutation-report.html" \
     --arg json "$MUTATION_DIR/reports/mutation-report.json" \
     '.thresholds.break = 0
      | .reporters = ["html", "json", "clear-text"]
      | .htmlReporter.fileName = $html
      | .jsonReporter.fileName = $json' \
    stryker.conf.json > "$CONFIG_FILE"
fi

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
fi

# The normalized result is a side effect - the script's own exit code must still be
# Stryker's, so a real failure (or a broken threshold) fails the calling make target.
exit $STRYKER_EXIT_CODE
