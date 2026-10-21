#!/bin/bash
# Checks that every variation point of the architecture catalog attaches to its
# base without editing it: for each library/architecture/*/*.skill/*.allow, the
# example it names (`examples <dir>`, default examples/) may add any file, but
# may change or delete a file of the base example only when the allow file
# names it (`change <glob>`, `delete <glob>`). A skill with a second mode has a
# second allow file for its second example (attach-proto.allow).
# Prints what each variation point adds, changes, and deletes. A file two
# variation points of one base both add (internal/api/health/health.go) must be
# the same in both, or attaching both to one application conflicts.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

# files lists a directory's files, relative to it, without build and test output
# (Go: bin/ and tmp/ at the root; dotnet: bin/, obj/, TestResults/ in every
# project and the code Reqnroll generates from each .feature).
files() {
    (cd "$1" && find . -type f \
        -not -path './tmp/*' -not -path '*/bin/*' -not -path '*/obj/*' \
        -not -path '*/TestResults/*' -not -path '*/node_modules/*' -not -name '*.feature.cs' \
        | sed 's#^\./##' | sort)
}

# allowed reports whether path matches one of the globs after the given verb.
allowed() {
    local verb=$1 path=$2 allow=$3 pattern
    while read -r kind pattern; do
        [ "$kind" = "$verb" ] || continue
        # shellcheck disable=SC2053 # the pattern is a glob on purpose
        [[ $path == $pattern ]] && return 0
    done < <(grep -v '^#' "$allow")
    return 1
}

status=0
declare -A first_adder=()
for allow in library/architecture/*/*.skill/*.allow; do
    [ -e "$allow" ] || continue
    skill=$(dirname "$allow")
    base=$(awk '$1 == "base" {print $2}' "$allow")
    examples="$skill/$(awk '$1 == "examples" {print $2}' "$allow")"
    [ "$examples" = "$skill/" ] && examples="$skill/examples"
    echo "== $(basename "$skill" .skill) ($(basename "$examples")) attaches to $base"
    added=() changed=() deleted=() bad=()
    while read -r f; do
        if [ ! -e "$base/$f" ]; then
            added+=("$f")
            key="$base|$f"
            if [ -n "${first_adder[$key]:-}" ]; then
                cmp -s "${first_adder[$key]}/$f" "$examples/$f" \
                    || bad+=("added differently by ${first_adder[$key]}: $f")
            else
                first_adder[$key]=$examples
            fi
        elif ! cmp -s "$base/$f" "$examples/$f"; then
            changed+=("$f")
            allowed change "$f" "$allow" || bad+=("changed, not allowed: $f")
        fi
    done < <(files "$examples")
    while read -r f; do
        if [ ! -e "$examples/$f" ]; then
            deleted+=("$f")
            allowed delete "$f" "$allow" || bad+=("deleted, not allowed: $f")
        fi
    done < <(files "$base")
    echo "   added   ${#added[@]}: ${added[*]:-}"
    echo "   changed ${#changed[@]}: ${changed[*]:-}"
    echo "   deleted ${#deleted[@]}: ${deleted[*]:-}"
    for b in "${bad[@]:-}"; do
        [ -n "$b" ] && { echo "   ERROR $b" >&2; status=1; }
    done
done
[ "$status" = 0 ] && echo "vp attach ok: no variation point edits its base beyond attach.allow; shared files are identical"
exit "$status"
