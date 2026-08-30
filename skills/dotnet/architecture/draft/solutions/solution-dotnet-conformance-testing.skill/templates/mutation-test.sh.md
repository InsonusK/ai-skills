# scripts/mutation-test.sh

Runs Stryker.NET — its native `--since` mode covers `ONLY_DELTA`/`DELTA_BASE` directly, so this script does not need to compute the diff itself. See [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|solution-conformance-testing]] for the target contract.

```bash
#!/usr/bin/env bash
# Runs Stryker.NET mutation testing and normalizes the results into tmp/result/*.json,
# keeping the native browsable report under tmp/report/.
#
# Params (env vars):
#   ONLY_DELTA=true   only mutate code changed since DELTA_BASE (for PRs); default is a
#                     full run, which never gates (--break-at 0) since it's report-only -
#                     the PR job is what enforces the threshold, via ONLY_DELTA.
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
  # consult Stryker.NET's current docs for the exact "since" flag/config key
  STRYKER_ARGS+=(--since:"$DELTA_BASE")
else
  # Full run has no PR base to diff against, so the whole module is mutated; the break
  # threshold is overridden to 0 so a low score never fails this run - it only reports
  # the score, it doesn't gate anything. The PR job (ONLY_DELTA=true) enforces the real
  # threshold from stryker-config.json before code reaches master.
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

# The normalized result is a side effect - the script's own exit code must still be
# Stryker's, so a real failure (or a broken threshold) fails the calling make target.
exit $STRYKER_EXIT_CODE
```
