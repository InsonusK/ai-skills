#!/usr/bin/env bash
# badges: components
set -euo pipefail
source tools/testing/kind.sh
source tools/testing/nx-env.sh
kind_mode "Nx components: all applicable projects; cache disabled"
if [ "$TEST_RUN_PURPOSE" = check ] && [ -n "$DELTA_BASE" ]; then
  kind_mode "Nx components: affected projects since $DELTA_BASE; cache disabled"
fi
status=0
node tools/testing/nx-kind.mjs run components || status=$?
reason=$(jq -r '.skipReason // empty' "$RESULT_DIR/projects.json")
[ -z "$reason" ] || kind_skip "$reason"
node tools/testing/nx-kind.mjs native components
