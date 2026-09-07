# scripts/mutation-test.sh

`mutmut`'s CLI for CI-friendly result export and for scoping a run to specific changed files has moved between major versions more than Stryker.NET/StrykerJS have — treat every `mutmut` line below as a sketch to verify against the version this project pins, not a copy-paste command. The surrounding contract (env vars, `tmp/result/mutation-test.json` schema, exit-code propagation) is what must hold regardless of which `mutmut` version/flags end up filling it in — see [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract).

```bash
#!/usr/bin/env bash
# Runs mutmut mutation testing and normalizes the results into
# tmp/result/mutation-test.json, keeping the native browsable report under
# tmp/report/mutation.
#
# Params (env vars):
#   ONLY_DELTA=true   only mutate source files changed since DELTA_BASE (for PRs);
#                     default is a full run, which never gates since it's report-only -
#                     the PR job is what enforces the threshold, via ONLY_DELTA.
#   DELTA_BASE=<ref>  git ref to diff against; required when ONLY_DELTA=true.
set -euo pipefail

ONLY_DELTA="${ONLY_DELTA:-false}"
DELTA_BASE="${DELTA_BASE:-}"

RESULT_DIR="tmp/result"
REPORT_DIR="tmp/report/mutation"

mkdir -p "$RESULT_DIR"
rm -rf "$REPORT_DIR" .mutmut-cache

MUTMUT_PATHS=()
if [ "$ONLY_DELTA" = "true" ]; then
  if [ -z "$DELTA_BASE" ]; then
    echo "DELTA_BASE is required when ONLY_DELTA=true" >&2
    exit 1
  fi

  FILES=$(git diff --name-only --diff-filter=ACMR "$DELTA_BASE" HEAD -- '{package}/**/*.py')
  if [ -z "$FILES" ]; then
    echo "No changes in {package}/**/*.py since $DELTA_BASE — skipping mutation testing."
    exit 0
  fi
  # VERIFY: confirm the current mutmut version's actual flag/config key for limiting a
  # run to specific paths (this may require a temporary [tool.mutmut] override instead
  # of a CLI flag) - do not trust this flag name without checking mutmut's own docs.
  MUTMUT_PATHS=(--paths-to-mutate "$(echo "$FILES" | paste -sd, -)")
fi

set +e
mutmut run "${MUTMUT_PATHS[@]}"
MUTMUT_EXIT_CODE=$?
set -e

# VERIFY: confirm the current mutmut version's command for a machine-readable result
# export (e.g. `mutmut results`, a junitxml/html exporter) - the counts below assume a
# report that classifies each mutant the same way Stryker.NET/StrykerJS do (killed,
# survived, timed out, not covered by any test).
mutmut html
if [ -d html ]; then
  mv html "$REPORT_DIR"
fi

# Replace this block with real parsing once the export command above is confirmed.
KILLED=0
SURVIVED=0
TIMEDOUT=0
NO_COVERAGE=0
TESTED=$((KILLED + SURVIVED + TIMEDOUT + NO_COVERAGE))
if [ "$TESTED" -eq 0 ]; then
  SCORE="0.0"
else
  SCORE=$(awk -v k="$KILLED" -v t="$TESTED" 'BEGIN { printf "%.1f", (k / t) * 100 }')
fi
printf '{"killed":%s,"survived":%s,"timedout":%s,"noCoverage":%s,"score":%s}' \
  "$KILLED" "$SURVIVED" "$TIMEDOUT" "$NO_COVERAGE" "$SCORE" > "$RESULT_DIR/mutation-test.json"

# The normalized result is a side effect - the script's own exit code must still be
# mutmut's, so a real failure (or a broken threshold) fails the calling make target.
exit $MUTMUT_EXIT_CODE
```
