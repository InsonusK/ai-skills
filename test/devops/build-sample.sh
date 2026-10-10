#!/bin/bash
# test/devops/build-sample.sh - builds a sample repository the DevOps skills are tried on:
# the Go example of the testing skills, with every file the DevOps skills deliver added the
# way an agent applying them would add it.
#   bash test/devops/build-sample.sh OUT-DIR [project type for assemble-workflow.sh ...]
set -euo pipefail
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
out=${1:?usage: build-sample.sh OUT-DIR [type...]}; shift
D=$root/skills/devops
assemble=$D/core/devops-ci-orchestration.skill/scripts/assemble-workflow.sh

rm -rf "$out"; mkdir -p "$out"
cp -R "$root/skills/testing/go/solution-conformance-testing-in-go.skill/examples/." "$out/"
rm -rf "$out/tmp"

# devops-project-version + -in-go
mkdir -p "$out/tools/version"
cp "$D"/core/devops-project-version.skill/assets/tools/version/* "$out/tools/version/"
cp "$D"/go/devops-project-version-in-go.skill/assets/tools/version/read-version.sh "$out/tools/version/"
grep -q 'tools/version/version.mk' "$out/Makefile" || printf '\ninclude tools/version/version.mk\n' >> "$out/Makefile"

# devops-ci-changes-in-go, devops-ci-toolchain-in-go
cp -R "$D"/go/devops-ci-changes-in-go.skill/assets/.github "$out/"
cp -R "$D"/go/devops-ci-toolchain-in-go.skill/assets/.github "$out/"

# devops-github-wf-pull-request, devops-github-wf-release
mkdir -p "$out/.github/workflows"
pr_types=(); for t in "$@"; do [ "$t" = docker ] && pr_types+=(docker); done
sh "$assemble" "$D/workflows/devops-github-wf-pull-request.skill/templates/pull-request.yml" "${pr_types[@]}" > "$out/.github/workflows/pull-request.yml"
sh "$assemble" "$D/workflows/devops-github-wf-release.skill/templates/release.yml" "$@" > "$out/.github/workflows/release.yml"
echo "sample built in $out"
