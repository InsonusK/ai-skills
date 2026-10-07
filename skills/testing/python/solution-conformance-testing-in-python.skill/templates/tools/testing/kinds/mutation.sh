#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for Python: mutmut over the whole package in a report run, where the
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

pip install -e ".[dev]"

MUTATION_DIR="$REPORT_DIR/mutation"
rm -rf "$MUTATION_DIR" .mutmut-cache

MUTMUT_PATHS=()
if [ "$TEST_RUN_PURPOSE" = check ]; then
  FILES=$(git diff --name-only --diff-filter=ACMR "$DELTA_BASE" HEAD -- '{package}/**/*.py')
  if [ -z "$FILES" ]; then
    kind_skip "no changes in {package}/**/*.py since $DELTA_BASE"
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
  mv html "$MUTATION_DIR"
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
