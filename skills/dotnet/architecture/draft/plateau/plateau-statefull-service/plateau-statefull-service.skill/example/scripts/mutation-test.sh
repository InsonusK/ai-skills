#!/usr/bin/env bash
# Runs Stryker.NET mutation testing and normalizes the results into tmp/result/*.json,
# keeping the native browsable report under tmp/report/.
#
# Params (env vars):
#   ONLY_DELTA=true   only mutate code changed since DELTA_BASE (for PRs); default is a
#                     full run, which never gates (--break-at 0).
#   DELTA_BASE=<ref>  git ref to diff against; required when ONLY_DELTA=true.
set -euo pipefail

ONLY_DELTA="${ONLY_DELTA:-false}"
DELTA_BASE="${DELTA_BASE:-}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report/mutation"

mkdir -p "$RESULT_DIR"
rm -rf "$REPORT_DIR"

STRYKER_ARGS=(-r html -r json -r cleartext -O "$REPORT_DIR" --break-on-initial-test-failure)
if [ "$ONLY_DELTA" = "true" ]; then
  if [ -z "$DELTA_BASE" ]; then
    echo "DELTA_BASE is required when ONLY_DELTA=true" >&2
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
