#!/bin/bash
# Checks that every code template of the architecture and develop catalogs is
# the same as the file of a runnable example once its placeholders are filled:
# a template is copied into real projects, an example is what the scenarios
# test, and the two must not drift. A template is compared with the file at the
# same path under its own skill's examples*/ (examples-proto/ for a file of the
# proto mode), or, for a skill with no example (error-kinds-in-go), under the
# first example of the catalog that has it. Go templates carry placeholders
# (fill); Python templates name the package {package}, in their imports and in
# their path (src/{package}/...); dotnet templates (.cs, .csproj) have none and
# are compared as they are.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

# The values the examples use for every placeholder.
fill() {
    sed -e 's#{module-path}#github.com/example/todo#g' \
        -e 's#{contract-name}#todo#g' \
        -e 's#{contract-package}#todo#g' \
        -e 's#{api-title}#Todo#g' \
        -e 's#{package}#todo#g' \
        -e 's#{version}#0.1.0#g' "$1"
}

status=0 checked=0
for templates in library/architecture/*/*.skill/templates library/develop/*/*.skill/templates; do
    [ -d "$templates" ] || continue
    skill=$(dirname "$templates")
    while read -r f; do
        # a Python template's path names its package {package}, the examples' todo
        at=${f//\{package\}/todo}
        # every example of the skill that has the file; else the first of the catalog
        examples=$(ls "$skill"/examples*/"$at" 2>/dev/null || true)
        if [ -z "$examples" ]; then
            examples=$(ls library/architecture/*/*.skill/examples*/"$at" 2>/dev/null | head -1 || true)
        fi
        if [ -z "$examples" ]; then
            echo "ERROR $templates/$f: no example has this file" >&2
            status=1
            continue
        fi
        for example in $examples; do
            checked=$((checked + 1))
            if ! fill "$templates/$f" | cmp -s - "$example"; then
                echo "ERROR $templates/$f differs from $example once filled" >&2
                status=1
            fi
        done
    done < <(cd "$templates" && find . -type f \( -name '*.go' -o -name '*.cs' -o -name '*.csproj' -o -name '*.py' \) | sed 's#^\./##' | sort)
done
[ "$status" = 0 ] && echo "templates ok: $checked template-example pairs match"
exit "$status"
