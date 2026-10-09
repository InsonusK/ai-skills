#!/usr/bin/env bash
# Runs the testing contract in one example and checks what it leaves - INVARIANTS.md §5.
# Needs the example's toolchain (see .devcontainer), jq, node, python3.
#   bash skills/testing/agent/run-example.sh <example-dir>
# A: a report run            - exit 0, the whole report, every link of the landing page resolves
# B: a check run, own dirs   - nothing in tmp/ or public/, mutation skipped, only the tests badge
# C: delta mutation          - scoped to HEAD~1, or the kind skips itself
# Prints one line per check; exits non-zero when any failed. Leaves no output behind.
set -u
A=$(cd "$(dirname "$0")" && pwd)
cd "${1:?usage: run-example.sh <example-dir>}" || exit 2
log=$(mktemp -d); fail=0
ok()    { echo "  ok   $*"; }
bad()   { echo "  FAIL $*"; fail=1; }
has()   { [ -e "$1" ] && ok "$1" || bad "missing $1"; }
hasnt() { [ ! -e "$1" ] && ok "absent $1" || bad "unexpected $1"; }
run()   { local name=$1; shift; "$@" > "$log/$name.log" 2>&1; local rc=$?
          [ $rc -eq 0 ] && ok "$name: exit 0" || { bad "$name: exit $rc - $log/$name.log"; tail -5 "$log/$name.log" | sed 's/^/       | /'; }; }

echo "== $(pwd)"
rm -rf tmp out public
make -n init > /dev/null 2>&1 && run init make init   # an example that needs initializing has the target

echo "-- A: make test-and-report"
run A make test-and-report
R=tmp/testing/report
for f in index.html run.json badges/tests.json badges/coverage.json badges/mutation.json \
         reports/tests/index.html reports/coverage reports/mutation \
         reports/tests/livingdoc/index.html; do has "$R/$f"; done
hasnt public
hasnt "$R/reports/scenarios"
python3 "$A/livingdoc-check.py" tmp/testing/kinds/unit/result/scenarios.json "$R/reports/tests/livingdoc" || fail=1
python3 "$A/report-links.py" "$R" || fail=1
jq -r '.kinds[] | "  \(.kind): \(.state) - \(.note)"' "$R/run.json" 2>/dev/null
cat "$R"/badges/*.json 2>/dev/null | sed 's/}{/}\n{/g; s/^/  /'; echo

echo "-- B: check run, caller-chosen directories"
rm -rf tmp out public
run B make test-and-report TEST_RUN_PURPOSE=check TEST_WORK_DIR=out/work TEST_REPORT_DIR=out/site/testing
R=out/site/testing
hasnt tmp; hasnt public
has "$R/index.html"; has "$R/run.json"; has "$R/badges/tests.json"
hasnt "$R/reports/scenarios"
python3 "$A/livingdoc-check.py" out/work/kinds/unit/result/scenarios.json "$R/reports/tests/livingdoc" || fail=1
hasnt "$R/badges/coverage.json"; hasnt "$R/badges/mutation.json"; hasnt "$R/reports/coverage"; hasnt "$R/reports/mutation"
[ "$(jq -r '.kinds[] | select(.kind == "mutation").state' "$R/run.json" 2>/dev/null)" = skipped ] \
  && ok "mutation skipped" || bad "mutation not skipped"

echo "-- C: delta mutation"
run C make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=HEAD~1 TEST_WORK_DIR=out/work
cat out/work/kinds/mutation/{mode,skipped} 2>/dev/null | sed 's/^/  /'

rm -rf tmp out
[ $fail -eq 0 ] && rm -rf "$log"
exit $fail
