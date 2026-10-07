#!/usr/bin/env bash
# The mutation test kind: runs Stryker.NET and normalizes the results into
# $TEST_KIND_DIR/result/*.json, keeping the native browsable report under
# $TEST_KIND_DIR/report/mutation/. Called by `make test-kind-mutation`, which exports:
#   TEST_KIND_DIR      the only directory this kind writes to
#   TEST_RUN_PURPOSE   pr-check: only mutate code changed since DELTA_BASE, thresholds apply;
#                      report: the whole solution, which never fails on the score (--break-at 0)
#   DELTA_BASE         git ref to diff against in a pr-check run (make skips the kind without it)
set -euo pipefail

PURPOSE="${TEST_RUN_PURPOSE:-report}"
DELTA_BASE="${DELTA_BASE:-}"
KIND_DIR="${TEST_KIND_DIR:?run this through make test-kind-mutation}"

RESULT_DIR="$KIND_DIR/result"
REPORT_DIR="$KIND_DIR/report/mutation"

mkdir -p "$RESULT_DIR"
rm -rf "$REPORT_DIR"

STRYKER_ARGS=(-r html -r json -r cleartext -O "$REPORT_DIR" --break-on-initial-test-failure)
if [ "$PURPOSE" = "pr-check" ]; then
  if [ -z "$DELTA_BASE" ]; then
    echo "DELTA_BASE is required in a pr-check run" >&2
    exit 1
  fi
  STRYKER_ARGS+=(--since:"$DELTA_BASE")
else
  STRYKER_ARGS+=(--break-at 0)
fi

set +e
dotnet tool run dotnet-stryker "${STRYKER_ARGS[@]}"
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

exit $STRYKER_EXIT_CODE
