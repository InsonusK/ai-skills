#!/bin/bash
# test/devops/run-local.sh - runs, in a built sample, every `make` call the two workflows make,
# in the order of a pull request into master. What only GitHub can do - the path filter, the
# registry, Pages, the Release - is not covered here.
#   bash test/devops/run-local.sh SAMPLE-DIR
set -euo pipefail
ROOT=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "${1:?usage: run-local.sh SAMPLE-DIR}"
git init -q -b master . 2>/dev/null || true
git add -A && git -c user.name=sample -c user.email=sample@example.invalid commit -q -m base
git checkout -q -b feature

step() { echo; echo "== $*"; "$@"; }
step make -s version
echo "-- version-check must fail while the version is not raised"
if make -s version-check DELTA_BASE=master; then echo "FAIL: an unraised version passed"; exit 1; fi
next=$(make -s version | awk -F. '{ printf "%d.%d.%d", $1, $2 + 1, 0 }')
sed -i "s/^var Version = .*/var Version = \"$next\"/" internal/version/version.go
step make -s version-check DELTA_BASE=master
git checkout -q internal/version/version.go
step make test-readme-check
kinds=$(make -s test-kinds | cut -d' ' -f1 | jq -Rsc 'split("\n") | map(select(. != ""))')
echo "kinds=$kinds"
step make init
for kind in $(echo "$kinds" | jq -r '.[]'); do
  TEST_RUN_PURPOSE=check step make "test-kind-$kind"
done
# The naming step of the release action, for each channel - the part of it that runs without a registry.
for channel in check snapshot release; do
  echo; echo "== release action, channel $channel"
  GITHUB_OUTPUT=$(mktemp) GITHUB_REPOSITORY=Org/Sample INPUT_CHANNEL=$channel INPUT_VERSION=$(make -s version) INPUT_TIMESTAMP=20261010120000 \
    bash -c 'node "$0/skills/devops/agent/run-action-step.mjs" .github/actions/release/action.yml names | bash -e && cat "$GITHUB_OUTPUT"' "$ROOT"
done
echo; echo "run-local: every make call of the workflows passed"
