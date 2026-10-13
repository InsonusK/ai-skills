#!/bin/sh
# tools/version/version.sh - prints the project's version and checks that it was raised
# (devops-project-version). Copied verbatim into every project; never edited there.
# Where the version is recorded is known only to read-version.sh beside this file.
#
#   version.sh          print the version
#   version.sh check    fail unless the version is greater than the one at $DELTA_BASE
set -eu

here=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)

# Prints the version recorded in the project at directory $1.
# Returns 1 when none is recorded there, 2 when it is not MAJOR.MINOR.PATCH.
read_version() {
  v=$(cd "$1" 2>/dev/null && sh "$here/read-version.sh" 2>/dev/null) || return 1
  [ -n "$v" ] || return 1
  if ! printf '%s\n' "$v" | grep -Eq '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$'; then
    echo "version: '$v' is not MAJOR.MINOR.PATCH" >&2
    return 2
  fi
  printf '%s\n' "$v"
}

# Succeeds when version $1 is greater than version $2.
greater() {
  old_ifs=$IFS; IFS=.
  set -- $1 $2
  IFS=$old_ifs
  [ "$1" -ne "$4" ] && { [ "$1" -gt "$4" ]; return; }
  [ "$2" -ne "$5" ] && { [ "$2" -gt "$5" ]; return; }
  [ "$3" -gt "$6" ]
}

current=$(read_version .) || {
  [ $? -eq 2 ] || echo "version: no version is recorded in this project" >&2
  exit 1
}

case "${1:-}" in
  "")
    echo "$current"
    ;;
  check)
    if [ -z "${DELTA_BASE:-}" ]; then
      echo "version-check: DELTA_BASE is not set - the ref to compare with" >&2
      exit 2
    fi
    if ! git rev-parse --verify --quiet "$DELTA_BASE^{commit}" >/dev/null; then
      echo "version-check: DELTA_BASE '$DELTA_BASE' is not a commit of this repository" >&2
      exit 2
    fi
    # The same reader, run over a checkout of the base: no second copy of where the version is.
    prefix=$(git rev-parse --show-prefix)
    tmp=$(mktemp -d)
    trap 'git worktree remove --force "$tmp/base" >/dev/null 2>&1; rm -rf "$tmp"' EXIT
    git worktree add --detach --quiet "$tmp/base" "$DELTA_BASE"
    if ! base=$(read_version "$tmp/base/$prefix" 2>/dev/null); then
      echo "version-check: $current - $DELTA_BASE records no version to compare with"
      exit 0
    fi
    if greater "$current" "$base"; then
      echo "version-check: $current is greater than $base at $DELTA_BASE"
    else
      echo "version-check: $current must be greater than $base at $DELTA_BASE" >&2
      exit 1
    fi
    ;;
  *)
    echo "usage: version.sh [check]" >&2
    exit 2
    ;;
esac
