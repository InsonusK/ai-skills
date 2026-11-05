#!/bin/bash
# Applies to every template that check-templates.sh reports the change its
# example got since HEAD (git diff of the example file, patched onto the
# template), so a template keeps its placeholders.
cd "$(git rev-parse --show-toplevel)" || exit 1
bash test/check-templates.sh 2>&1 | sed -n 's/^ERROR \(.*\) differs from \(.*\) once filled$/\1 \2/p' | while read -r template example; do
    if git diff --quiet HEAD -- "$example"; then echo "SKIP $template: its example has no change"; continue; fi
    if git diff HEAD -- "$example" | sed -e "s#github.com/example/todo#{module-path}#g" -e "s#from todo\\.#from {package}.#g" | patch --quiet --no-backup-if-mismatch "$template"; then
        echo "patched $template"
    else
        echo "FAILED $template"
    fi
done
bash test/check-templates.sh >/dev/null 2>&1 && echo "templates ok"
