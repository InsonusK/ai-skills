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

code=0
"$GREMLINS" unleash --coverpkg="$COVERPKG" --exclude-files='gen/.*' --exclude-files='tools/.*' "${run_args[@]}" \
  --output "$REPORT_DIR/mutation/gremlins.json" . || code=$?
go run ./tools/normalize_mutation "$REPORT_DIR/mutation/gremlins.json"
exit "$code"
