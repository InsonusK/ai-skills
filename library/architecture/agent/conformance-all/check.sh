#!/bin/bash
# The checks of giving every spec skill its shared scenarios (INVARIANTS.md):
# every .feature of a stack example is a copy of a spec skill's conformance/
# or named -in-{stack}; every spec skill has conformance/ and says so; the
# repository's checks of shared scenarios and attached VPs pass. --ground
# also runs the tests of every example (slow).
set -uo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
cd "$ROOT"
A=library/architecture
status=0
fail() { echo "ERROR $*" >&2; status=1; }

# 1. Every spec skill keeps conformance/ and states the rule (INVARIANTS.md 2, 5).
for spec in $A/spec/*.skill; do
    name=$(basename "$spec" .skill)
    [ -d "$spec/conformance" ] || { fail "$spec: no conformance/"; continue; }
    grep -q 'conformance.copy' "$spec/$name.skill.md" || fail "$spec/$name.skill.md: no rule naming conformance.copy"
    while read -r f; do
        case "$f" in *.feature|*.proto) ;; *) fail "$f: not a scenario or its input" ;; esac
        b=$(basename "$f")
        [[ $b =~ ^[a-z0-9]+(-[a-z0-9]+)*\.(feature|proto)$ ]] || fail "$f: not kebab-case"
    done < <(find "$spec/conformance" -type f)
done

# 2. Every stack skill has conformance.copy (INVARIANTS.md 3).
for skill in $A/{go,python,dotnet}/*.skill; do
    [ -f "$skill/conformance.copy" ] || fail "$skill: no conformance.copy"
done

# 3. Every .feature of an example is a copy or the stack's own (INVARIANTS.md 1).
copies=$(mktemp)
while read -r manifest; do
    skill=$(dirname "$manifest")
    while read -r source destination; do
        (cd "$A/spec/$source" 2>/dev/null && find . -type f | sed "s#^\./#$skill/$destination/#") >>"$copies"
    done < <(grep -v '^[[:space:]]*\(#\|$\)' "$manifest")
done < <(find $A -name conformance.copy)
while read -r f; do
    stack=$(echo "$f" | cut -d/ -f3)
    [[ $(basename "$f") == *-in-$stack.feature ]] && continue
    grep -qxF "$f" "$copies" || fail "$f: neither a shared copy nor named -in-$stack"
done < <(find $A/{go,python,dotnet} -name '*.feature' -not -path '*/bin/*' -not -path '*/obj/*' -not -path '*/.venv/*' | sort)
rm -f "$copies"

# 4. The repository's checks.
bash test/check-conformance.sh >/dev/null || fail "test/check-conformance.sh"
bash test/check-vp-attach.sh >/dev/null || fail "test/check-vp-attach.sh"
bash test/check-templates.sh >/dev/null || fail "test/check-templates.sh"

# 5. --ground [skill-glob]: the tests of every example.
if [ "${1:-}" = "--ground" ]; then
    for example in $A/{go,python,dotnet}/${2:-*}.skill/examples*; do
        [ -d "$example" ] || continue
        case "$example" in
            *vp-postgres*|*vp-unit-of-work*) db=1 ;; *) db=0 ;;
        esac
        case "$example" in
            */go/*) (cd "$example" && go vet ./... && go test -count=1 ./... >/dev/null) || fail "$example: go vet or go test" ;;
            */python/*) (cd "$example" && { [ -d .venv ] || make init >/dev/null 2>&1; } && make test >/dev/null && make lint >/dev/null) || fail "$example: make test or make lint" ;;
            */dotnet/*) (cd "$example" && dotnet test --nologo -v q >/dev/null) || fail "$example: dotnet test" ;;
        esac
    done
fi

[ "$status" = 0 ] && echo "conformance-all ok"
exit "$status"
