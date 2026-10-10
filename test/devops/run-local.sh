#!/bin/bash
# test/devops/run-local.sh - runs, in a built sample, every `make` call the two workflows make,
# in the order of a pull request into master. What only GitHub can do - the path filter, the
# registry, Pages, the Release - is not covered here.
#   bash test/devops/run-local.sh SAMPLE-DIR
set -euo pipefail
cd "${1:?usage: run-local.sh SAMPLE-DIR}"
git init -q -b master . 2>/dev/null || true
git add -A && git -c user.name=sample -c user.email=sample@example.invalid commit -q -m base
git checkout -q -b feature

step() { echo; echo "== $*"; "$@"; }
step make -s version
echo "-- version-check must fail while the version is not raised"
if make -s version-check DELTA_BASE=master; then echo "FAIL: an unraised version passed"; exit 1; fi
awk -F. '{ printf "%d.%d.%d\n", $1, $2 + 1, 0 }' VERSION > VERSION.new && mv VERSION.new VERSION
step make -s version-check DELTA_BASE=master
# The example has a scenario that pins its own version; the tests run on the committed one.
git checkout -q VERSION
step make test-readme-check
kinds=$(make -s test-kinds | cut -d' ' -f1 | jq -Rsc 'split("\n") | map(select(. != ""))')
echo "kinds=$kinds"
step make init
for kind in $(echo "$kinds" | jq -r '.[]'); do
  TEST_RUN_PURPOSE=check step make "test-kind-$kind"
done
echo; echo "run-local: every make call of the workflows passed"
