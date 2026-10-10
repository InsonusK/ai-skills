#!/bin/bash
# test/devops/push-sample.sh - puts the sample on GitHub to try pull-request.yml there:
#   develop-devops         an orphan branch whose root is the built sample (build-sample.sh)
#   devops-sample-change   the same with one changed source file - the head of the pull request
# The workflow is pointed at develop-devops instead of develop. release.yml is left out: the
# release workflow is not tried here.
#   bash test/devops/push-sample.sh
set -euo pipefail
root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
remote=$(git -C "$root" remote get-url origin)
work=$(mktemp -d); trap 'rm -rf "$work"' EXIT

bash "$root/test/devops/build-sample.sh" "$work/sample"
cd "$work/sample"
rm .github/workflows/release.yml
# The one edit of a file that is otherwise copied verbatim: the experiment's branch name.
sed -i 's/^      - develop$/      - develop-devops/' .github/workflows/pull-request.yml
git init -q -b develop-devops .
commit() { git add -A && git -c user.name="devops sample" -c user.email=sample@example.invalid commit -q -m "$1"; }
commit "sample: Go example of the testing skills with the DevOps skills applied"
git checkout -q -b devops-sample-change
file=$(git ls-files 'internal/**/*.go' | grep -v '_test.go' | grep -v '/test/' | head -n 1)
printf '\n// A change of code: the pull request must run the tests and need no version bump into develop.\n' >> "$file"
commit "sample: change one source file"
git push --force "$remote" develop-devops devops-sample-change
