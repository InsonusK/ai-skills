#!/usr/bin/env bash
# tools/testing/testing.sh - checks behind tools/testing/testing.mk (solution-conformance-testing).
# Copied verbatim into every project; never edited there. Called by make, which exports
# TEST_DECLARED ("<kind>:<badge>,<badge> ..."), TEST_RUN_PURPOSE, TEST_WORK_DIR, TEST_REPORT_DIR, TEST_README.
set -euo pipefail

declared_kinds()  { for d in $TEST_DECLARED; do echo "${d%%:*}"; done; }
declared_badges() { local d b; for d in $TEST_DECLARED; do [ "$1" = all ] || [ "$1" = "${d%%:*}" ] || continue
                      b=${d#*:}; [ -n "$b" ] && tr ',' '\n' <<<"$b"; done; return 0; }
kind_of_badge()   { local d; for d in $TEST_DECLARED; do case ",${d#*:}," in *",$1,"*) echo "${d%%:*}"; return;; esac; done; }

# README must show exactly the declared badges. A badge is recognised by "badges/<name>.json" in its URL.
readme_check() {
  local fail=0 name
  [ -f "$TEST_README" ] || { echo "test-readme-check: $TEST_README not found"; exit 1; }
  for name in $(declared_badges all | sort -u); do
    grep -q "badges/$name\.json" "$TEST_README" || {
      echo "test-readme-check: you forgot to add a badge to $TEST_README - test kind '$(kind_of_badge "$name")' produces badge '$name'; add a badge whose URL ends with badges/$name.json"
      fail=1; }
  done
  for name in $(grep -o 'badges/[A-Za-z0-9_-]*\.json' "$TEST_README" | sed 's|badges/||; s|\.json$||' | sort -u); do
    declared_badges all | grep -qx "$name" || {
      echo "test-readme-check: $TEST_README shows badge '$name', which no test kind produces - remove it or declare it in TEST_BADGES_<kind>"
      fail=1; }
  done
  [ $fail -eq 0 ] && echo "test-readme-check: $TEST_README shows every declared badge"
  exit $fail
}

# After test-report-build: record how the run went and check the report against the declaration.
report_finish() {
  local kinds_dir="$TEST_WORK_DIR/kinds" fail=0 kind state text name first=1 expected
  [ -f "$TEST_REPORT_DIR/index.html" ] || { echo "test-report: $TEST_REPORT_DIR/index.html is missing"; fail=1; }

  { printf '{ "purpose": "%s", "kinds": [' "$TEST_RUN_PURPOSE"
    for kind in $(declared_kinds); do
      if   [ -f "$kinds_dir/$kind/skipped" ]; then state=skipped; text=$(head -1 "$kinds_dir/$kind/skipped")
      elif [ -d "$kinds_dir/$kind" ];         then state=ran;     text=$(head -1 "$kinds_dir/$kind/mode" 2>/dev/null || true)
      else                                         state=missing; text="the kind left no result"
      fi
      [ $first -eq 1 ] || printf ','; first=0
      printf '\n  { "kind": "%s", "state": "%s", "note": "%s" }' "$kind" "$state" "$(sed 's/[\\"]/\\&/g' <<<"$text")"
      echo "test-report: $kind - $state${text:+ ($text)}" >&2
    done
    printf '\n] }\n'; } > "$TEST_REPORT_DIR/run.json"

  # Every badge has a report of the same name, and belongs to a declared kind.
  for name in $(find "$TEST_REPORT_DIR/badges" -maxdepth 1 -name '*.json' -exec basename {} .json \; | sort); do
    [ -d "$TEST_REPORT_DIR/reports/$name" ] || { echo "test-report: badge '$name' has no report $TEST_REPORT_DIR/reports/$name/"; fail=1; }
    declared_badges all | grep -qx "$name" || { echo "test-report: badge '$name' is produced but no test kind declares it in TEST_BADGES_<kind>"; fail=1; }
  done
  # A full run must produce every badge of every kind that ran.
  if [ "$TEST_RUN_PURPOSE" = report ]; then
    for kind in $(declared_kinds); do
      [ -d "$kinds_dir/$kind" ] && [ ! -f "$kinds_dir/$kind/skipped" ] || continue
      for expected in $(declared_badges "$kind"); do
        [ -f "$TEST_REPORT_DIR/badges/$expected.json" ] || { echo "test-report: test kind '$kind' declares badge '$expected' but the run produced none"; fail=1; }
      done
    done
  fi
  exit $fail
}

case "${1:-}" in
  readme-check)  readme_check ;;
  report-finish) report_finish ;;
  *) echo "usage: testing.sh readme-check|report-finish" >&2; exit 2 ;;
esac
