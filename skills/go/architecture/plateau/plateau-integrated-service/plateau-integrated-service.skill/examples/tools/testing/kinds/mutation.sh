#!/usr/bin/env bash
# badges: mutation
# The mutation test kind for Go: gremlins over the whole module in a report run, where the
# score never fails the run (both thresholds forced to 0); in a check run only over files
# changed since DELTA_BASE with the project's configured thresholds, and skipped without one.
# Exits with gremlins' own exit code after writing the normalized result.
set -euo pipefail
source tools/testing/kind.sh

run_args=()
if [ "$TEST_RUN_PURPOSE" = check ]; then
  [ -n "$DELTA_BASE" ] || kind_skip "a check run mutates only changed files and no DELTA_BASE was given"
  run_args=(--diff "$DELTA_BASE")
  kind_mode "mutating only files changed since $DELTA_BASE"
else
  # 0 switches a threshold off, also one set in the project's .gremlins.yaml.
  run_args=(--threshold-efficacy 0 --threshold-mcover 0)
  kind_mode "mutating the whole module - the score never fails the run"
fi

GREMLINS_VERSION=v0.6.0
GREMLINS="$(go env GOPATH)/bin/gremlins"
COVERPKG=$(go list ./... | grep -Ev '/(gen|tools)(/|$)' | tr '\n' ',' | sed 's/,$//')

[ -x "$GREMLINS" ] || go install "github.com/go-gremlins/gremlins/cmd/gremlins@$GREMLINS_VERSION"
mkdir -p "$REPORT_DIR/mutation"

# --integration runs the whole module's tests for each mutant. Without it gremlins runs only
# the tests of the mutated package - and that package has none: its scenarios run from the
# test/ package beside it, so every mutant that compiles would survive.
# GOFLAGS=-count=1 keeps `go test` from answering out of its cache: gremlins sizes every
# mutant's timeout from its first, unmutated run, and a cached run takes no time - the
# mutants of a slow suite would then all be reported as timed out.
# --timeout-coefficient 10: with gremlins' default a suite that runs in under a second gets a
# timeout shorter than rebuilding the test binaries takes, and killable mutants are reported
# as timed out.
# diff.relative makes git name changed files from this directory: gremlins compares them
# with module-relative paths, so without it --diff matches nothing in a module that sits
# below the repository root.
code=0
GOFLAGS="${GOFLAGS:+$GOFLAGS }-count=1" \
GIT_CONFIG_COUNT=1 GIT_CONFIG_KEY_0=diff.relative GIT_CONFIG_VALUE_0=true \
"$GREMLINS" unleash --integration --timeout-coefficient 10 --coverpkg="$COVERPKG" --exclude-files='gen/.*' --exclude-files='tools/.*' "${run_args[@]}" \
  --output "$REPORT_DIR/mutation/gremlins.json" . || code=$?
go run ./tools/normalize_mutation "$REPORT_DIR/mutation/gremlins.json"
kind_badge_percent mutation "mutation score" "$(jq '.score' "$RESULT_DIR/mutation-test.json")"
exit "$code"
