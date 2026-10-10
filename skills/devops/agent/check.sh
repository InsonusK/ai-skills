#!/usr/bin/env bash
# Mechanical check for skills/devops - see INVARIANTS.md. Run from the repository root:
#   bash skills/devops/agent/check.sh
# Exits non-zero on any failure. Needs bash, git, perl, awk, make; actionlint when installed.
set -u
cd "$(git rev-parse --show-toplevel)" || exit 2
A=skills/devops/agent
D=skills/devops
fail=0
err() { echo "FAIL: $*"; fail=1; }
stacks='angular|dotnet|go|python|typescript'

links=$(mktemp); trap 'rm -f "$links"' EXIT
git ls-files -co --exclude-standard '*.md' | grep -v "^$A/" | tr '\n' '\0' | xargs -0 perl skills/testing/agent/links.pl > "$links"

# 1. Every link from or into skills/devops resolves.
out=$(awk -F'\t' '$5=="broken" && ($1 ~ /^skills\/devops\// || $3 ~ /skills\/devops\//) && $1 !~ /service-deploy-skill-template\.md$/' "$links")
[ -n "$out" ] && { err "broken links from or into skills/devops:"; echo "$out" | cut -f1-3; }

# 2. Isolation (INVARIANTS §5): a skill under skills/devops links outside it only to testing and design skills.
out=$(awk -F'\t' '$1 ~ /^skills\/devops\// && $5=="ok" && $4 !~ /^skills\/(devops|testing|design)\// { print $1 "\t" $4 }' "$links" | sort -u)
[ -n "$out" ] && { err "links leaving skills/devops:"; echo "$out"; }

# 3. A stack-agnostic skill never links a stack-specialized one; a stack skill never links another stack's.
out=$(awk -F'\t' -v s="^skills/devops/($stacks)/" '$1 ~ /^skills\/devops\/(core|workflows)\// && $4 ~ s { print $1 "\t" $4 }' "$links" | sort -u)
[ -n "$out" ] && { err "stack-agnostic skill links a stack-specialized skill:"; echo "$out"; }
out=$(awk -F'\t' -v s="^skills/devops/($stacks)/" '$1 ~ s && $4 ~ s { split($1,a,"/"); split($4,b,"/"); if (a[3]!=b[3] && !(a[3]=="angular" && b[3]=="typescript")) print $1 "\t" $4 }' "$links" | sort -u)
[ -n "$out" ] && { err "stack skill links another stack's skill:"; echo "$out"; }

# 4. Layout: core/, workflows/, deploy/ hold stack-agnostic skills, {stack}/ holds {name}-in-{stack} skills.
skill_file() { local e=$1 b n; b=$(basename "$e"); n=${b%.md}; n=${n%.skill}; if [ -d "$e" ]; then echo "$e/$n.skill.md"; else echo "$e"; fi; }
for d in $D/*/; do
  dir=$(basename "$d"); [ "$dir" = agent ] && continue
  [[ "$dir" =~ ^(core|workflows|deploy|$stacks)$ ]] || { err "$d: not core/, workflows/, deploy/ or a stack folder"; continue; }
  for e in "$d"*; do
    b=$(basename "$e"); n=${b%.md}; n=${n%.skill}
    [[ "$b" == *.skill || "$b" == *.skill.md ]] || { err "$e: not a skill"; continue; }
    f=$(skill_file "$e"); [ -f "$f" ] || { err "$f: missing skill file"; continue; }
    grep -qx "name: $n" "$f" || err "$f: frontmatter name is not $n"
    grep -qE '^updated: [0-9]{8}$' "$f" || err "$f: no updated: YYYYMMDD"
    grep -qE '^\s*- concern/' "$f" || err "$f: no concern tag"
    if [[ "$dir" =~ ^($stacks)$ ]]; then
      [[ "$n" == *-in-$dir ]] || err "$e: name does not end with -in-$dir"
      want="stack/$dir"; [ "$dir" = angular ] && want="framework/angular"   # angular is a framework on stack/typescript
      grep -qE "^\s*- $want$" "$f" || err "$f: missing tag $want"
      base=${n%-in-$dir}
      if ls $D/core/$base.skill* >/dev/null 2>&1; then
        grep -q "$D/core/$base.skill" "$f" || err "$f: does not link its base $base"
      fi
    else
      [[ "$n" =~ -in-($stacks)$ ]] && err "$e: a stack-specific skill outside a stack folder"
      grep -qE '^\s*- stack$' "$f" || err "$f: missing bare stack tag"
    fi
    for h in '# Goal' '# Core Principle' '# Rule' '# Check list'; do
      [ "$(grep -cx "$h" "$f")" = 1 ] || err "$f: needs exactly one '$h'"
    done
  done
done

# Skills not yet rewritten (STATUS.md, W5-W7) are skipped by 5 and 7; the list only shrinks.
legacy() { grep -qF "$(echo "$1" | cut -d/ -f1-4)" "$A/legacy-paths.txt"; }

# 5. Every extension a base names in backticks exists.
for f in $(ls $D/core/*.skill/*.skill.md $D/core/*.skill.md $D/workflows/*.skill/*.skill.md 2>/dev/null); do
  legacy "$f" && continue
  for n in $(grep -oE '`devops-[a-z-]+-in-('"$stacks"')`' "$f" | tr -d '`' | sort -u); do
    s=${n##*-in-}
    ls $D/$s/$n.skill* >/dev/null 2>&1 || err "$f names $n, which does not exist in $D/$s/"
  done
done

# 6. INVARIANTS §1: no shipped workflow or action names a test tool, a coverage or mutation switch, or a version source.
forbidden='go test|go vet|pytest|mutmut|gremlins|stryker|dotnet test|coverlet|npm test|npx (cucumber|jest|vitest|playwright|ng)|ng test|godog|--cov|--coverage|-cover\b|cat VERSION|pyproject\.toml|package\.json|Directory\.Build\.props|test-kind-(unit|mutation|components|ui)\b'
for f in $(git ls-files -co --exclude-standard "$D/**/*.yml" "$D/**/*.yaml" | grep -vE "^$D/(deploy|devops-service-deploy\.skill)/"); do
  out=$(grep -nE "$forbidden" "$f" | grep -vE '^\s*[0-9]+:\s*#')
  [ -n "$out" ] && { err "$f names a test tool or a version source:"; echo "$out"; }
done

# 7. Code is delivered as files: no YAML or shell fence longer than 15 lines in a devops skill, no *.example.md.
for f in $(git ls-files -co --exclude-standard "$D/core/**/*.md" "$D/workflows/**/*.md" $(printf "$D/%s/**/*.md " angular dotnet go python typescript)); do
  legacy "$f" && continue
  awk -v f="$f" '/^\s*```(yaml|yml|bash|sh|shell)/ { n=0; inb=1; start=NR; next } /^\s*```/ { if (inb && n>15) printf "%s:%d: fenced code of %d lines\n", f, start, n; inb=0 } inb { n++ }' "$f"
done > "$links.fence"
[ -s "$links.fence" ] && { err "code that belongs in assets/ or templates/:"; cat "$links.fence"; }
rm -f "$links.fence"

# 8. A replaced skill is named nowhere (the list grows as waves remove skills).
while read -r name; do
  [ -z "$name" ] && continue
  out=$(git grep -lE "$name([^a-z-]|$)" -- skills ':!skills/devops/agent' ':!skills/testing/agent' ':!skills/design' 2>/dev/null)
  [ -n "$out" ] && { err "removed skill $name is still named in:"; echo "$out"; }
done < "$A/removed-skills.txt"

# 9. Delivered scripts parse; delivered workflows and actions lint.
for f in $(git ls-files -co --exclude-standard "$D/**/assets/**/*.sh" "$D/**/templates/**/*.sh"); do
  sh -n "$f" 2>/dev/null || bash -n "$f" || err "$f: does not parse"
done
if command -v actionlint >/dev/null 2>&1; then
  wf=$(git ls-files -co --exclude-standard "$D/**/*.yml" | grep '/\.github/workflows/')
  [ -n "$wf" ] && { actionlint -shellcheck= $wf || err "actionlint"; }
else
  echo "note: actionlint is not installed - workflows are not linted"
fi

# 10. Ground truth: the version tools of every stack.
bash "$A/fixtures.sh" >/tmp/devops-fixtures.log 2>&1 || { err "fixtures.sh:"; grep -E 'FAIL|fixtures' /tmp/devops-fixtures.log; }

# 11. Ground truth: the check-changes actions against sample paths, matched as dorny/paths-filter matches.
if [ -d "$A/node_modules/picomatch" ]; then
  node "$A/changes-fixtures.mjs" >/tmp/devops-changes.log 2>&1 || { err "changes-fixtures.mjs:"; grep FAIL /tmp/devops-changes.log; }
else
  echo "note: run 'npm install' in $A - the check-changes patterns are not tested"
fi
# The actions differ between stacks only in the lines that carry the test patterns and the skill's name.
ref=
for f in $D/*/devops-ci-changes-in-*.skill/assets/.github/actions/check-changes/action.yml; do
  n=$(grep -vE "^# Source:|^          # |- '!?\{.*(test|Tests)/\*\*" "$f")
  [ -z "$ref" ] && { ref=$n; continue; }
  [ "$n" = "$ref" ] || err "$f: differs from the other stacks outside its test patterns"
done

[ "$fail" -eq 0 ] && echo "check: all passed" || exit 1
