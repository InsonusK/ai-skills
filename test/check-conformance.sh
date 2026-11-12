#!/bin/bash
# Checks that every stack skill runs the shared conformance scenarios of its
# spec skill as they are: a spec skill keeps them in its conformance/ folder,
# and a stack skill names in its conformance.copy where its example takes a
# copy of each file. A copy that is missing or differs from its source fails
# the check, so a skill cannot reach master with scenarios it does not run.
#
#   bash test/check-conformance.sh          check every copy
#   bash test/check-conformance.sh --sync   write every copy from its source
#                                           (make conformance-sync)
#
# A line of conformance.copy is "<source> <destination>": the source is a
# conformance/ folder relative to library/architecture/spec, the destination a
# folder relative to the stack skill. Every file of the source is copied; the
# destination may hold more files of its own (a stack's own scenarios).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

sync=false
[ "${1:-}" = "--sync" ] && sync=true

SPEC=library/architecture/spec
status=0 checked=0
while read -r manifest; do
    skill=$(dirname "$manifest")
    while read -r source destination; do
        if [ ! -d "$SPEC/$source" ]; then
            echo "ERROR $manifest: no folder $SPEC/$source" >&2
            status=1
            continue
        fi
        while read -r f; do
            from="$SPEC/$source/$f" to="$skill/$destination/$f"
            checked=$((checked + 1))
            if $sync; then
                mkdir -p "$(dirname "$to")"
                cp "$from" "$to"
            elif [ ! -f "$to" ]; then
                echo "ERROR $to is missing: run make conformance-sync" >&2
                status=1
            elif ! cmp -s "$from" "$to"; then
                echo "ERROR $to differs from $from: run make conformance-sync" >&2
                status=1
            fi
        done < <(cd "$SPEC/$source" && find . -type f | sed 's#^\./##' | sort)
    done < <(grep -v '^[[:space:]]*\(#\|$\)' "$manifest")
done < <(find library/architecture -name conformance.copy | sort)

if [ "$status" = 0 ]; then
    if $sync; then echo "conformance synced: $checked files copied"; else echo "conformance ok: $checked copies match"; fi
fi
exit "$status"
