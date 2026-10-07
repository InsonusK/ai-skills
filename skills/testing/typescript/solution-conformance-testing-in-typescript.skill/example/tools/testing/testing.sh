#!/usr/bin/env bash
# tools/testing/testing.sh - runs the testing contract (solution-conformance-testing).
# Copied verbatim into every project; never edited there. Called by tools/testing/testing.mk.
#
#   testing.sh kinds          one line per kind: "<kind> <badge> <badge> ..."
#   testing.sh kind <kind>    run tools/testing/kinds/<kind>.sh in its own directory
#   testing.sh report         build the report directory and check it
#   testing.sh readme-check   the README shows exactly the declared badges
#   testing.sh all            every kind, then the report
set -euo pipefail

KINDS_SRC=tools/testing/kinds
export TEST_RUN_PURPOSE="${TEST_RUN_PURPOSE:-report}"
export DELTA_BASE="${DELTA_BASE:-}"
TEST_WORK_DIR="${TEST_WORK_DIR:-tmp/testing}"
TEST_REPORT_DIR="${TEST_REPORT_DIR:-$TEST_WORK_DIR/report}"
TEST_README="${TEST_README:-README.md}"
# Absolute, so a kind may change directory freely.
export TEST_WORK_DIR="$(realpath -m "$TEST_WORK_DIR")"
export TEST_REPORT_DIR="$(realpath -m "$TEST_REPORT_DIR")"

case "$TEST_RUN_PURPOSE" in check|report) ;; *)
  echo "TEST_RUN_PURPOSE must be check or report, got '$TEST_RUN_PURPOSE'" >&2; exit 2 ;; esac

kinds()  { local f; for f in "$KINDS_SRC"/*.sh; do [ -f "$f" ] && basename "$f" .sh; done; return 0; }
# The badges a kind produces in a full run: its script's "# badges: a b" line.
badges() { sed -n 's/^# badges:[[:space:]]*//p' "$KINDS_SRC/$1.sh" | head -1; }
all_badges()    { local k; for k in $(kinds); do badges "$k" | tr ' ' '\n'; done | sed '/^$/d' | sort -u; }
kind_of_badge() { local k; for k in $(kinds); do case " $(badges "$k") " in *" $1 "*) echo "$k"; return;; esac; done; }

run_kind() {
  local kind="$1" script="$KINDS_SRC/$1.sh" status=0
  [ -f "$script" ] || { echo "test-kind-$kind: no such kind - $script is missing (kinds: $(kinds | tr '\n' ' '))" >&2; exit 2; }
  export TEST_KIND="$kind" TEST_KIND_DIR="$TEST_WORK_DIR/kinds/$kind"
  rm -rf "$TEST_KIND_DIR" && mkdir -p "$TEST_KIND_DIR/result" "$TEST_KIND_DIR/report"
  bash "$script" || status=$?
  # Kept beside the kind's results: the report tells a failed kind from one that ran.
  echo "$status" > "$TEST_KIND_DIR/exit-code"
  return "$status"
}

# ran: the kind's script exited 0. failed: it exited non-zero, or never finished.
kind_state() {
  [ "$(cat "$TEST_WORK_DIR/kinds/$1/exit-code" 2>/dev/null)" = 0 ] && echo ran || echo failed
}

# README must show exactly the declared badges. A badge is recognised by "badges/<name>.json" in its URL.
readme_check() {
  local fail=0 name
  [ -f "$TEST_README" ] || { echo "test-readme-check: $TEST_README not found"; exit 1; }
  for name in $(all_badges); do
    grep -q "badges/$name\.json" "$TEST_README" || {
      echo "test-readme-check: you forgot to add a badge to $TEST_README - test kind '$(kind_of_badge "$name")' produces badge '$name'; add a badge whose URL ends with badges/$name.json"
      fail=1; }
  done
  for name in $(grep -o 'badges/[A-Za-z0-9_-]*\.json' "$TEST_README" | sed 's|badges/||; s|\.json$||' | sort -u); do
    all_badges | grep -qx "$name" || {
      echo "test-readme-check: $TEST_README shows badge '$name', which no test kind produces - remove it or declare it in the kind's '# badges:' line"
      fail=1; }
  done
  [ $fail -eq 0 ] && echo "test-readme-check: $TEST_README shows every declared badge"
  exit $fail
}

report() {
  local kinds_dir="$TEST_WORK_DIR/kinds" fail=0 kind state text name first=1 expected
  rm -rf "$TEST_REPORT_DIR" && mkdir -p "$TEST_REPORT_DIR/reports" "$TEST_REPORT_DIR/badges"
  bash tools/testing/test-report.sh

  # How the run went: the purpose, and for each kind what it did because of it.
  { printf '{ "purpose": "%s", "kinds": [' "$TEST_RUN_PURPOSE"
    for kind in $(kinds); do
      if   [ -f "$kinds_dir/$kind/skipped" ]; then state=skipped; text=$(head -1 "$kinds_dir/$kind/skipped")
      elif [ -d "$kinds_dir/$kind" ];         then state=$(kind_state "$kind"); text=$(head -1 "$kinds_dir/$kind/mode" 2>/dev/null || true)
      else                                         state=missing; text="the kind left no result"
      fi
      [ $first -eq 1 ] || printf ','; first=0
      printf '\n  { "kind": "%s", "state": "%s", "note": "%s" }' "$kind" "$state" "$(sed 's/[\\"]/\\&/g' <<<"$text")"
      echo "test-report: $kind - $state${text:+ ($text)}" >&2
    done
    printf '\n] }\n'; } > "$TEST_REPORT_DIR/run.json"

  [ -f "$TEST_REPORT_DIR/index.html" ] || { echo "test-report: $TEST_REPORT_DIR/index.html is missing"; fail=1; }
  # Every badge has a report of the same name, and belongs to a declared kind.
  for name in $(find "$TEST_REPORT_DIR/badges" -maxdepth 1 -name '*.json' -exec basename {} .json \; | sort); do
    [ -d "$TEST_REPORT_DIR/reports/$name" ] || { echo "test-report: badge '$name' has no report reports/$name/"; fail=1; }
    all_badges | grep -qx "$name" || { echo "test-report: badge '$name' is produced but no kind declares it in its '# badges:' line"; fail=1; }
  done
  # A full run must produce every badge of every kind that ran. A failed kind may have
  # stopped before its result - its own exit code already reported that.
  if [ "$TEST_RUN_PURPOSE" = report ]; then
    for kind in $(kinds); do
      [ -d "$kinds_dir/$kind" ] && [ ! -f "$kinds_dir/$kind/skipped" ] && [ "$(kind_state "$kind")" = ran ] || continue
      for expected in $(badges "$kind"); do
        [ -f "$TEST_REPORT_DIR/badges/$expected.json" ] || { echo "test-report: test kind '$kind' declares badge '$expected' but the run produced none"; fail=1; }
      done
    done
  fi
  exit $fail
}

case "${1:-}" in
  kinds)        for k in $(kinds); do echo "$k $(badges "$k")"; done ;;
  kind)         run_kind "${2:?usage: testing.sh kind <kind>}" ;;
  report)       report ;;
  readme-check) readme_check ;;
  all)          status=0
                for k in $(kinds); do ( run_kind "$k" ) || status=$?; done
                ( report ) || status=$?
                exit $status ;;
  *) echo "usage: testing.sh kinds|kind <kind>|report|readme-check|all" >&2; exit 2 ;;
esac
