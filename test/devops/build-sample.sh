#!/bin/bash
# test/devops/build-sample.sh - builds a sample repository the DevOps skills are tried on:
# the Go example of the testing skills, with every file the DevOps skills deliver added the
# way an agent applying them would add it.
#   bash test/devops/build-sample.sh OUT-DIR [RELEASE-SKILL]
# RELEASE-SKILL: the release action the sample takes, default devops-release-tag-only
# (the example is a library with no cmd/).
set -euo pipefail
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
out=${1:?usage: build-sample.sh OUT-DIR [RELEASE-SKILL]}
release=${2:-devops-release-tag-only}
D=$root/skills/devops

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

# devops-github-wf-pull-request, devops-github-wf-release, and one release action
cp -R "$D"/core/devops-github-wf-pull-request.skill/assets/.github "$out/"
cp -R "$D"/core/devops-github-wf-release.skill/assets/.github "$out/"
cp -R "$D"/*/"$release".skill/assets/.github "$out/"
echo "sample built in $out"
