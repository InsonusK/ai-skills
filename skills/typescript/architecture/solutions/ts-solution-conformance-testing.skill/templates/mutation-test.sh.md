# scripts/mutation-test.sh

StrykerJS has no native `--since`/delta flag the way Stryker.NET does, so this script emulates `ONLY_DELTA` itself by limiting `--mutate` to the files `git diff` reports as changed. See [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract) for the target contract.

```bash
#!/usr/bin/env bash
# Runs StrykerJS mutation testing and normalizes the results into
# tmp/result/mutation-test.json, keeping the native browsable report under
# tmp/report/mutation.
#
# Params (env vars):
#   ONLY_DELTA=true   only mutate source files changed since DELTA_BASE (for PRs);
#                     default is a full run, which never gates (break threshold
#                     overridden to 0) since it's report-only - the PR job is what
#                     enforces the threshold, via ONLY_DELTA.
#   DELTA_BASE=<ref>  git ref to diff against; required when ONLY_DELTA=true.
set -euo pipefail

ONLY_DELTA="${ONLY_DELTA:-false}"
DELTA_BASE="${DELTA_BASE:-}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report/mutation"

mkdir -p "$RESULT_DIR"
rm -rf "$REPORT_DIR"

MUTATE_ARGS=()
if [ "$ONLY_DELTA" = "true" ]; then
  if [ -z "$DELTA_BASE" ]; then
    echo "DELTA_BASE is required when ONLY_DELTA=true" >&2
    exit 1
  fi

  FILES=$(git diff --name-only --diff-filter=ACMR "$DELTA_BASE" HEAD -- 'src/**/*.ts' | paste -sd, -)
  if [ -z "$FILES" ]; then
    echo "No changes in src/**/*.ts since $DELTA_BASE — skipping mutation testing."
    exit 0
  fi
  MUTATE_ARGS=(--mutate "$FILES")
fi

CONFIG_FILE="$(mktemp --suffix=.json)"
trap 'rm -f "$CONFIG_FILE"' EXIT

if [ "$ONLY_DELTA" = "true" ]; then
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
  # the score, it doesn't gate anything. The PR job (ONLY_DELTA=true) enforces the real
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
```
