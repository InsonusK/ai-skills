#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for .NET: Stryker.NET over the whole solution in a report run, where
# the score never fails the run (--break-at 0); in a check run only over production files
# changed since DELTA_BASE with the configured thresholds, and skipped without one or when
# none changed.
set -euo pipefail
source tools/testing/kind.sh

MUTATION_DIR="$REPORT_DIR/mutation"
STRYKER_ARGS=(-r html -r json -r cleartext -O "$MUTATION_DIR" --break-on-initial-test-failure)
if [ "$TEST_RUN_PURPOSE" = check ]; then
  [ -n "$DELTA_BASE" ] || kind_skip "a check run mutates only changed code and no DELTA_BASE was given"
  SINCE=$(git rev-parse --verify --quiet "$DELTA_BASE^{commit}") \
    || { echo "DELTA_BASE '$DELTA_BASE' does not name a commit" >&2; exit 2; }
  # One --mutate pattern per changed production file, relative to its project. Stryker's own
  # --since is not used: with Reqnroll-generated tests it takes every changed file for a
  # test file and ignores its mutants.
  CHANGED=0
  while IFS= read -r file; do
    dir=$(dirname "$file")
    while [ "$dir" != . ] && ! compgen -G "$dir/*.csproj" > /dev/null; do dir=$(dirname "$dir"); done
    compgen -G "$dir/*.csproj" > /dev/null || continue        # not part of a project
    compgen -G "$dir/*Tests.csproj" > /dev/null && continue   # a test project is not mutated
    STRYKER_ARGS+=(--mutate "${file#"$dir"/}")
    CHANGED=$((CHANGED + 1))
  done < <(git diff --relative --name-only --diff-filter=ACMR "$SINCE" -- '*.cs')
  [ "$CHANGED" -gt 0 ] || kind_skip "no production .cs file changed since $DELTA_BASE"
  kind_mode "mutating only the $CHANGED production file(s) changed since $DELTA_BASE"
else
  STRYKER_ARGS+=(--break-at 0)
  kind_mode "mutating the whole solution - the score never fails the run"
fi

dotnet tool restore

set +e
dotnet tool run dotnet-stryker "${STRYKER_ARGS[@]}"
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

exit $STRYKER_EXIT_CODE
