# tools/testing/kind.sh - sourced by every tools/testing/kinds/<kind>.sh (solution-conformance-testing).
# Copied verbatim into every project; never edited there.
#
# A kind script gets from the environment:
#   TEST_KIND_DIR      the only directory it may write to; result/ and report/ exist and are empty
#   TEST_RUN_PURPOSE   check | report        DELTA_BASE   a ref, or empty
# It writes result/*.json and report/<name>/, says what the purpose changed through
# kind_mode (or leaves through kind_skip), and exits with its tool's own exit code.

: "${TEST_KIND_DIR:?run this through make test-kind-<kind>}"
RESULT_DIR="$TEST_KIND_DIR/result"
REPORT_DIR="$TEST_KIND_DIR/report"

# kind_mode <text> - what the kind does because of the run's purpose: to the log and the report.
kind_mode() {
  echo "test-kind-$TEST_KIND [purpose $TEST_RUN_PURPOSE]: $1"
  echo "$1" > "$TEST_KIND_DIR/mode"
}

# kind_skip <reason> - the kind does not apply to this run: no result, no badge, exit 0.
kind_skip() {
  echo "test-kind-$TEST_KIND [purpose $TEST_RUN_PURPOSE]: skipped - $1"
  echo "$1" > "$TEST_KIND_DIR/skipped"
  exit 0
}

# kind_livingdoc - render report/tests/cucumber/ (the runner's standard Cucumber report) into
# report/tests/livingdoc/ with the shared tools/livingdoc. Never fails the kind; skipped without npm.
kind_livingdoc() {
  if command -v npm >/dev/null 2>&1; then
    { npm ci --prefix tools/livingdoc --silent \
        && node tools/livingdoc/render.mjs "$REPORT_DIR/tests/cucumber" "$REPORT_DIR/tests/livingdoc"; } \
      || echo "livingdoc: render failed"
  else
    echo "livingdoc: npm not found - skipping living-doc report"
  fi
}
