#!/bin/bash
# skills/devops/agent/fixtures.sh - ground truth for tools/version/ (INVARIANTS.md §6).
# For every stack that ships a read-version.sh: a throw-away git repository with the shared
# assets copied in, then every answer `make version` and `make version-check` can give.
#   bash skills/devops/agent/fixtures.sh [stack...]
set -uo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
shared=$root/core/devops-project-version.skill/assets/tools/version
fail=0

# write_version <stack> <version> - records the version the way the stack's skill says.
write_version() {
  case "$1" in
    go) printf '%s\n' "$2" > VERSION ;;
    python) printf '[project]\nname = "fixture"\nversion = "%s"\n\n[tool.other]\nversion = "9.9.9"\n' "$2" > pyproject.toml ;;
    typescript) printf '{\n  "name": "fixture",\n  "version": "%s",\n  "dependencies": { "x": "9.9.9" }\n}\n' "$2" > package.json ;;
    dotnet) printf '<Project>\n  <PropertyGroup>\n    <Version>%s</Version>\n    <LangVersion>12.0</LangVersion>\n  </PropertyGroup>\n</Project>\n' "$2" > Directory.Build.props ;;
    *) echo "fixtures.sh: no write_version for stack '$1'" >&2; exit 2 ;;
  esac
}

# expect <label> <wanted exit code> <wanted text in the output> <command...>
expect() {
  local label=$1 code=$2 text=$3 out rc
  shift 3
  out=$("$@" 2>&1); rc=$?
  if [ "$rc" -eq "$code" ] && [[ "$out" == *"$text"* ]]; then
    echo "  ok    $label"
  else
    echo "  FAIL  $label: exit $rc (wanted $code), output: $out"; fail=1
  fi
}

commit() { git add -A && git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "$1"; }

run_stack() {
  local stack=$1 reader=$root/$1/devops-project-version-in-$1.skill/assets/tools/version/read-version.sh
  local repo; repo=$(mktemp -d)
  echo "$stack"
  (
    # The project sits in a subfolder of the repository, as a sample under test/ does.
    cd "$repo" && git init -q -b master . && mkdir -p app/tools/version && cd app || exit 2
    echo "before the project had a version" > README.md
    commit "no version yet"; git tag none
    cp "$shared"/version.mk "$shared"/version.sh "$reader" tools/version/
    echo 'include tools/version/version.mk' > Makefile
    write_version "$stack" 1.9.0; commit "1.9.0"; git tag base

    expect "version prints the recorded version"   0 "1.9.0" make -s version
    expect "an unchanged version fails the check"  2 "must be greater than 1.9.0" make -s version-check DELTA_BASE=base
    write_version "$stack" 1.10.0
    expect "1.10.0 is greater than 1.9.0"          0 "1.10.0 is greater than 1.9.0" make -s version-check DELTA_BASE=base
    write_version "$stack" 1.8.9
    expect "a lowered version fails the check"     2 "must be greater" make -s version-check DELTA_BASE=base
    write_version "$stack" 2.0.0
    expect "a major bump passes"                   0 "2.0.0 is greater" make -s version-check DELTA_BASE=base
    expect "no version at the base passes"         0 "records no version" make -s version-check DELTA_BASE=none
    expect "a missing DELTA_BASE is an error"      2 "DELTA_BASE is not set" make -s version-check
    expect "an unknown DELTA_BASE is an error"     2 "is not a commit" make -s version-check DELTA_BASE=nowhere
    write_version "$stack" 1.2
    expect "a two-part version is refused"         2 "is not MAJOR.MINOR.PATCH" make -s version
    write_version "$stack" 1.2.3-rc1
    expect "a suffixed version is refused"         2 "is not MAJOR.MINOR.PATCH" make -s version
    write_version "$stack" 2.0.0
    expect "the check leaves no worktree behind"   0 "1" bash -c 'make -s version-check DELTA_BASE=base >/dev/null; git worktree list | wc -l'
    exit $fail
  ) || fail=1
  rm -rf "$repo"
}

if [ $# -gt 0 ]; then stacks=("$@"); else
  stacks=()
  for d in "$root"/*/devops-project-version-in-*.skill; do [ -d "$d" ] && stacks+=("$(basename "$(dirname "$d")")"); done
fi
for s in "${stacks[@]}"; do run_stack "$s"; done
[ "$fail" -eq 0 ] && echo "fixtures: all passed" || { echo "fixtures: FAILED"; exit 1; }
