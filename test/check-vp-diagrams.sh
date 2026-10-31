#!/bin/bash
# Checks that every variation point of the architecture catalog is drawn as
# skill vp-diagrams requires: each spec skill imports diagrams/concept.mmd, each
# stack skill imports diagrams/example-flow.mmd, and each participant of an
# example flow that names a file names a file of the skill's example.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

status=0
fail() { echo "   ERROR $1" >&2; status=1; }

# imports reports whether a skill's file imports its diagram, and the diagram exists.
imports() {
    local skill=$1 diagram=$2 name
    name=$(basename "$skill" .skill)
    [ -f "$skill/diagrams/$diagram.mmd" ] || { fail "$skill has no diagrams/$diagram.mmd"; return; }
    grep -q "@import \"./diagrams/$diagram.mmd\"" "$skill/$name.skill.md" || fail "$name does not import diagrams/$diagram.mmd"
}

count=0
for skill in library/architecture/spec/*.skill; do
    imports "$skill" concept
    count=$((count + 1))
done
for skill in library/architecture/go/*.skill library/architecture/dotnet/*.skill library/architecture/python/*.skill; do
    imports "$skill" example-flow
    count=$((count + 1))
    flow="$skill/diagrams/example-flow.mmd"
    [ -f "$flow" ] || continue
    # "participant X as Name<br/>path/of/the/file"
    while read -r file; do
        [ -f "$skill/examples/$file" ] || fail "$flow names $file, not in its example"
    done < <(sed -n 's#^ *participant .*<br/>\(.*\)$#\1#p' "$flow")
done
[ "$status" = 0 ] && echo "vp diagrams ok: $count skills drawn, every participant's file exists"
exit "$status"
