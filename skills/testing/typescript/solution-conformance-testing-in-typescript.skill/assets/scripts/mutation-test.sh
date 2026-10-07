#!/usr/bin/env bash
# The mutation test kind.
# Runs StrykerJS mutation testing and normalizes the results into
# $TEST_KIND_DIR/result/mutation-test.json, keeping the native browsable report under
# $TEST_KIND_DIR/report/mutation. Called by `make test-kind-mutation`, which exports:
#   TEST_KIND_DIR      the only directory this kind writes to
#   TEST_RUN_PURPOSE   check: only mutate source files changed since DELTA_BASE, the
#                      real threshold applies; report: the whole package, which never
#                      fails on the score
#   DELTA_BASE         git ref to diff against in a check run (make skips the kind
#                      without it)
set -euo pipefail

PURPOSE="${TEST_RUN_PURPOSE:-report}"
DELTA_BASE="${DELTA_BASE:-}"
KIND_DIR="${TEST_KIND_DIR:?run this through make test-kind-mutation}"

RESULT_DIR="$KIND_DIR/result"
REPORT_DIR="$KIND_DIR/report/mutation"

mkdir -p "$RESULT_DIR"
rm -rf "$REPORT_DIR"

MUTATE_ARGS=()
if [ "$PURPOSE" = "check" ]; then
  if [ -z "$DELTA_BASE" ]; then
    echo "DELTA_BASE is required in a check run" >&2
    exit 1
  fi

  FILES=$(git diff --name-only --diff-filter=ACMR "$DELTA_BASE" HEAD -- 'src/**/*.ts' | paste -sd, -)
  if [ -z "$FILES" ]; then
    echo "no changes in src/**/*.ts since $DELTA_BASE" > "$KIND_DIR/skipped"
    echo "No changes in src/**/*.ts since $DELTA_BASE — skipping mutation testing."
    exit 0
  fi
  MUTATE_ARGS=(--mutate "$FILES")
fi

CONFIG_FILE="$(mktemp --suffix=.json)"
trap 'rm -f "$CONFIG_FILE"' EXIT

if [ "$PURPOSE" = "check" ]; then
  # Real threshold from stryker.conf.json applies here - the score has to be good
  # enough to pass the PR gate.
  jq --arg html "$REPORT_DIR/reports/mutation-report.html" \
     --arg json "$REPORT_DIR/reports/mutation-report.json" \
     '.reporters = ["html", "json", "clear-text"]
      | .htmlReporter.fileName = $html
      | .jsonReporter.fileName = $json' \
    stryker.conf.json > "$CONFIG_FILE"
else
  # Full run has no PR base to diff against, so the whole package is mutated; the break
  # threshold is overridden to 0 so a low score never fails this run - it only reports
  # the score, it doesn't gate anything. A check run enforces the real
  # threshold from stryker.conf.json before code reaches master.
  jq --arg html "$REPORT_DIR/reports/mutation-report.html" \
     --arg json "$REPORT_DIR/reports/mutation-report.json" \
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

MUTATION_JSON="$REPORT_DIR/reports/mutation-report.json"
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
