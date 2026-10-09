#!/usr/bin/env bash
# badges: ui
set -euo pipefail
source tools/testing/kind.sh
kind_mode "all browser UI tests; reviewed visual baselines remain read-only"
mkdir -p "$REPORT_DIR/ui" "$TEST_KIND_DIR/cache" "$TEST_KIND_DIR/tmp"
export TMPDIR="$TEST_KIND_DIR/tmp"
export PWTEST_CACHE_DIR="$TEST_KIND_DIR/cache"
set +e
./node_modules/.bin/playwright test --config=playwright.ui.config.ts \
  2>&1 | tee "$REPORT_DIR/ui/runner.log"
runner_exit=${PIPESTATUS[0]}
node tools/testing/angular-results.mjs ui "$runner_exit"
result_exit=$?
set -e
exit "$result_exit"
