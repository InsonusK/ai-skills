#!/usr/bin/env bash
# badges: components
set -euo pipefail
source tools/testing/kind.sh
kind_mode "all Angular component tests; native coverage in the component report"
mkdir -p "$REPORT_DIR/components" "$TEST_KIND_DIR/cache" "$TEST_KIND_DIR/tmp"
export TMPDIR="$TEST_KIND_DIR/tmp"
export XDG_CACHE_HOME="$TEST_KIND_DIR/cache"
set +e
./node_modules/.bin/ng test linkcheck --watch=false --runner=vitest \
  --runner-config=vitest.components.config.mts --include='**/spec/*.component.spec.ts' \
  --coverage --reporters=json --output-file="$RESULT_DIR/components.native.json" \
  2>&1 | tee "$REPORT_DIR/components/runner.log"
runner_exit=${PIPESTATUS[0]}
node tools/testing/angular-results.mjs components "$runner_exit"
result_exit=$?
set -e
exit "$result_exit"
