#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for Python: mutmut over the whole package in a report run; in a
# check run only over the source files changed since DELTA_BASE, and skipped without one or
# when none changed. mutmut has no score threshold - it exits non-zero only when it could
# not run (a red test fails its clean run), so the score never fails either run.
set -euo pipefail
source tools/testing/kind.sh

NAMES=()
if [ "$TEST_RUN_PURPOSE" = check ]; then
  [ -n "$DELTA_BASE" ] || kind_skip "a check run mutates only changed files and no DELTA_BASE was given"
  SINCE=$(git rev-parse --verify --quiet "$DELTA_BASE^{commit}") \
    || { echo "DELTA_BASE '$DELTA_BASE' does not name a commit" >&2; exit 2; }
  # mutmut selects mutants by name - "<module path>.x_<function>__mutmut_<n>" - so a changed
  # file becomes the pattern "<module path>.x*". Test code is never mutated.
  while IFS= read -r file; do
    case "/$file" in */test/*|*/tests/*|*/features/*|*/tools/*|*_test.py|*/conftest.py) continue ;; esac
    name=${file%.py}; name=${name%/__init__}; name=${name//\//.}
    NAMES+=("${name#src.}.x*")
  done < <(git diff --relative --name-only --diff-filter=ACMR "$SINCE" -- '*.py')
  [ "${#NAMES[@]}" -gt 0 ] || kind_skip "no source .py file changed since $DELTA_BASE"
  kind_mode "mutating only the ${#NAMES[@]} source file(s) changed since $DELTA_BASE"
else
  kind_mode "mutating the whole package"
fi

pip install --quiet -e ".[dev]"

MUTATION_DIR="$REPORT_DIR/mutation"
mkdir -p "$MUTATION_DIR"
# mutmut works in ./mutants and takes no other place; it is removed again below.
rm -rf mutants

code=0
mutmut run "${NAMES[@]}" 2>&1 | tee "$MUTATION_DIR/mutmut.log" || code=$?
# The changed files hold nothing mutmut mutates: no mutants, not a failure.
if [ "$code" -ne 0 ] && grep -q "nothing matches" "$MUTATION_DIR/mutmut.log"; then
  echo "test-kind-$TEST_KIND: the changed files hold nothing to mutate"
  code=0
fi

if [ "$code" -eq 0 ]; then
  mutmut results --all true > "$MUTATION_DIR/results.txt"
  mutmut export-cicd-stats > /dev/null
  STATS=mutants/mutmut-cicd-stats.json
  KILLED=$(jq '.killed' "$STATS")
  SURVIVED=$(jq '.survived + .suspicious' "$STATS")
  TIMEDOUT=$(jq '.timeout' "$STATS")
  NO_COVERAGE=$(jq '.no_tests' "$STATS")
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
rm -rf mutants

exit "$code"
