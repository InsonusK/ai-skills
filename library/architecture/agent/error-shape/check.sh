#!/bin/bash
# The checks of the migration to one shape of error (INVARIANTS.md): what must
# be gone, what must be there, and the repository's own checks of templates,
# attached variation points, and shared scenarios. --ground also runs the
# tests and the type check of every example in scope (slow; Go and Python
# need TEST tools only, dotnet needs the SDK).
set -uo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
cd "$ROOT"
status=0
fail() { echo "ERROR $*" >&2; status=1; }

# The skills in scope: every stack skill of the catalog but the DB ones, and
# the error-kinds skills (INVARIANTS.md section 5).
scope=()
for skill in library/architecture/{go,python,dotnet}/*.skill library/develop/{go,python,dotnet}/error-kinds-in-*.skill; do
    case "$skill" in *vp-postgres-in-*|*vp-unit-of-work-in-*) continue ;; esac
    scope+=("$skill")
done

# 1. No kind wrapped by fmt.Errorf in Go: an error with a kind is errorkind.New or Wrap.
kinds='Err(Invalid|NotFound|Conflict|ConcurrentChange|FailedPrecondition|Unauthenticated|PermissionDenied|ResourceExhausted|Unimplemented|Unavailable)\b'
# A call may span lines, so each file is read whole. A technical outcome of
# an infrastructure (outcome.ErrUnavailable) is not a kind and may be wrapped;
# so may an error of a kind, by context ("%s: %w", operation, err).
while read -r hit; do fail "$hit: a kind wrapped by fmt.Errorf; use errorkind.New or errorkind.Wrap"; done < <(
    find "${scope[@]}" -name '*.go' -print0 2>/dev/null | xargs -0 perl -0777 -ne '
        while (/fmt\.Errorf\(\s*"%w[^"]*"((?:[^()]|\((?1)\))*?)[\s(,](?:(?:(?:entity|errorkind|pipeline|commandpipeline)\.)?'"$kinds"'|kinds\[[^\]]*\]|kind(?!\w))/g) {
            my $line = 1 + (substr($_, 0, $-[0]) =~ tr/\n//);
            print "$ARGV:$line\n";
        }')

# 2. The kinds live in the shared area, not in the domain.
while read -r file; do fail "$file: the kinds moved to the shared area (INVARIANTS.md section 2)"; done < <(
    find "${scope[@]}" -path '*/.venv' -prune -o \( -path '*/domain/entity/kinds.go' -o -path '*/domain/entity/kinds.py' \) -print 2>/dev/null)
while read -r hit; do fail "$hit: a kind read from the domain; read it from errorkind or the pipeline"; done < <(
    grep -rnE --include=*.go "\bentity\.$kinds" "${scope[@]}" 2>/dev/null)
while read -r hit; do fail "$hit: a kind imported from the domain; import it from shared.errors or the pipeline"; done < <(
    grep -rnE --include=*.py "from [a-z_{}]+\.domain\.entity(\.kinds)? import .*\b(Kind|KindError|kind_of)\b" "${scope[@]}" 2>/dev/null | grep -v '/\.venv/')

# 3. Every example in scope has the shared error.
for skill in "${scope[@]}"; do
    for example in "$skill"/examples*; do
        [ -d "$example" ] || continue
        case "$skill" in
            */go/*) [ -f "$example/internal/shared/errorkind/errorkind.go" ] || fail "$example: no internal/shared/errorkind/errorkind.go" ;;
            */python/*) [ -f "$example/src/todo/shared/errors/kinds.py" ] || fail "$example: no src/todo/shared/errors/kinds.py" ;;
        esac
    done
done

# 4. The repository's checks of what the migration touches.
bash test/check-templates.sh >/dev/null || fail "test/check-templates.sh"
bash test/check-vp-attach.sh >/dev/null || fail "test/check-vp-attach.sh"
bash test/check-conformance.sh >/dev/null || fail "test/check-conformance.sh"

# 5. --ground: the tests and the type check of every example in scope.
if [ "${1:-}" = "--ground" ]; then
    for skill in "${scope[@]}"; do
        for example in "$skill"/examples*; do
            [ -d "$example" ] || continue
            case "$skill" in
                */go/*) (cd "$example" && go vet ./... && go test -count=1 ./... >/dev/null) || fail "$example: go vet or go test" ;;
                */python/*) (cd "$example" && { [ -d .venv ] || make init >/dev/null 2>&1; } && make test >/dev/null && make lint >/dev/null) || fail "$example: make test or make lint" ;;
                */dotnet/*) (cd "$example" && dotnet test --nologo -v q >/dev/null) || fail "$example: dotnet test" ;;
            esac
        done
    done
fi

[ "$status" = 0 ] && echo "error shape ok: ${#scope[@]} skills in scope"
exit "$status"
