#!/usr/bin/env bash
# Mechanical check for skills/testing — see INVARIANTS.md. Run from the repository root:
#   bash skills/testing/agent/check.sh
# Exits non-zero on any failure. Needs bash, git, perl, awk.
set -u
cd "$(git rev-parse --show-toplevel)" || exit 2
A=skills/testing/agent
fail=0
err() { echo "FAIL: $*"; fail=1; }

links=$(mktemp); trap 'rm -f "$links"' EXIT
git ls-files -co --exclude-standard '*.md' | grep -v "^$A/" | tr '\n' '\0' | xargs -0 perl "$A/links.pl" > "$links"

# 1. Every link from or into skills/testing resolves.
out=$(awk -F'\t' '$5=="broken" && ($1 ~ /^skills\/testing\// || $3 ~ /skills\/testing\//)' "$links")
[ -n "$out" ] && { err "broken links from or into skills/testing:"; echo "$out" | cut -f1-3; }

# 2. Isolation: a skill under skills/testing links only inside skills/testing.
#    Known conflicts are listed in isolation-exceptions.tsv (source <TAB> target) until resolved.
out=$(awk -F'\t' 'FILENAME==ARGV[1] { ex[$1 "\t" $2]=1; next }
     $1 ~ /^skills\/testing\// && $5=="ok" && $4 !~ /^skills\/testing\// && !(($1 "\t" $4) in ex) { print $1 "\t" $4 }' \
     "$A/isolation-exceptions.tsv" "$links" | sort -u)
[ -n "$out" ] && { err "links leaving skills/testing:"; echo "$out"; }
out=$(awk -F'\t' 'NR==FNR { seen[$1 "\t" $4]=1; next } !(($1 "\t" $2) in seen) { print }' "$links" "$A/isolation-exceptions.tsv")
[ -n "$out" ] && { err "stale entries in isolation-exceptions.tsv:"; echo "$out"; }

# 3. A stack-agnostic skill never links a stack-specialized one.
out=$(awk -F'\t' '$1 ~ /^skills\/testing\// && $1 ~ /^skills\/testing\/core\// && $4 !~ /^skills\/testing\/core\// { print $1 "\t" $4 }' "$links" | sort -u)
[ -n "$out" ] && { err "stack-agnostic skill links a stack-specialized skill:"; echo "$out"; }
out=$(awk -F'\t' '$1 ~ /^skills\/testing\/(angular|dotnet|go|python|typescript)\// && $4 ~ /^skills\/testing\// { split($1,s,"/"); split($4,t,"/"); if (t[3]!="core" && t[3]!=s[3] && !(s[3]=="angular" && t[3]=="typescript")) print $1 "\t" $4 }' "$links" | sort -u)
[ -n "$out" ] && { err "stack skill links another stack's skill:"; echo "$out"; }

# 4. Layout: skills/testing/core/ holds stack-agnostic skills, skills/testing/{stack}/ holds {name}-in-{stack} skills;
#    the skill file's name: matches; a stack skill carries its stack tag, an agnostic one the bare stack tag.
stacks='angular|dotnet|go|python|typescript'
for d in skills/testing/*/; do
  dir=$(basename "$d"); [ "$dir" = agent ] && continue
  [[ "$dir" == core || "$dir" =~ ^($stacks)$ ]] || { err "$d: not core/ or a stack folder"; continue; }
  for e in "$d"*; do
    b=$(basename "$e"); n=${b%.md}; n=${n%.skill}
    [[ "$b" == *.skill || "$b" == *.skill.md ]] || { err "$e: not a skill"; continue; }
    f=$e; [ -d "$e" ] && f="$e/$n.skill.md"
    [ -f "$f" ] || { err "$f: missing skill file"; continue; }
    grep -qx "name: $n" "$f" || err "$f: frontmatter name is not $n"
    if [ "$dir" = core ]; then
      [[ "$n" =~ -in-($stacks)$ ]] && err "$e: a stack-specific skill in core/"
      grep -qE "^\s*- stack$" "$f" || err "$f: missing bare stack tag"
    else
      [[ "$n" == *-in-$dir ]] || err "$e: name does not end with -in-$dir"
      want="stack/$dir"; [ "$dir" = angular ] && want="framework/angular"   # angular is a framework on stack/typescript
      grep -qE "^\s*- $want$" "$f" || err "$f: missing tag $want"
    fi
  done
done

# 5. No reference to the old locations or the old names.
out=$(git grep -nIE 'skills/(common-workflow|angular|dotnet|go|python|typescript)/test/[a-z]|cucmber|no-test-theater-(angular|dotnet|python)\b|\bdotnet-unittest\b' \
      -- . ':!.validation' ":!$A" ':!*/agent/DECISIONS.md' ':!*/adr/*' | cut -c1-200)
[ -n "$out" ] && { err "references to the old test locations/names:"; echo "$out"; }

# 6. Shared contract files are verbatim copies of the core skill's Implementation blocks, in every example.
block() { perl -0ne 'print $1 if /^`{3,}\w*\n(.*?)^`{3,}$/ms' "$1"; }
core=skills/testing/core/solution-conformance-testing.skill/Implementation/tools
for f in testing/testing.mk testing/testing.sh livingdoc/render.mjs; do
  ref=$(block "$core/$f.create.md" | md5sum)
  while IFS= read -r copy; do
    [ "$(md5sum < "$copy")" = "$ref" ] || err "$copy differs from $core/$f.create.md"
  done < <(git ls-files -co --exclude-standard "skills/*/example/tools/$f" "skills/**/example/tools/$f")
done

# 7. Every example on the contract: its Makefile parses, lists kinds, and its README shows every declared badge.
while IFS= read -r mk; do
  dir=$(dirname "$mk")
  kinds=$(make -s -C "$dir" test-kinds 2>/dev/null) || { err "$dir: make test-kinds fails"; continue; }
  [ -n "$kinds" ] || err "$dir: make test-kinds prints nothing"
  make -s -C "$dir" test-readme-check >/dev/null 2>&1 || err "$dir: make test-readme-check fails"
  for k in $(echo "$kinds" | cut -d' ' -f1); do
    make -n -C "$dir" "test-kind-$k" >/dev/null 2>&1 || err "$dir: no target test-kind-$k"
  done
done < <(git grep -l 'include tools/testing/testing.mk' -- 'skills/**/example/Makefile')

# 8. Stack variants share their report scripts byte for byte.
for f in normalize-scenarios.sh test-report.sh; do
  n=$( { for s in dotnet python typescript; do block "skills/testing/$s/solution-conformance-testing-in-$s.skill/templates/$f.md" | md5sum; done; } | sort -u | wc -l)
  [ "$n" = 1 ] || err "templates/$f.md differs between the dotnet, python and typescript variants"
done

# 9. No caller-facing remnant of the previous contract.
out=$(git grep -nE 'make unit-test|make mutation-test|ONLY_DELTA|WITH_CODE_COVERAGE \?=|-badge\.json|tmp/result/|public/(tests|coverage|mutation|scenarios)' \
      -- skills ':!*/adr/*' ':!*/agent/*' ':!*/recheck/*' | cut -c1-200)
[ -n "$out" ] && { err "references to the previous testing contract:"; echo "$out"; }

# 10. Example tooling equals the stack skill's Implementation: Go tools, dotnet scripts ({Solution} = Sample).
go=skills/testing/go/solution-conformance-testing-in-go.skill/Implementation/tools
for t in normalize_unittest normalize_scenarios normalize_mutation test_report; do
  ref=$(block "$go/$t/main.go.create.md" | md5sum)
  while IFS= read -r copy; do
    [ "$(md5sum < "$copy")" = "$ref" ] || err "$copy differs from $go/$t/main.go.create.md"
  done < <(git ls-files -co --exclude-standard "skills/go/**/example/tools/$t/main.go")
done
dn=skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/templates
for s in unit-test.sh mutation-test.sh normalize-scenarios.sh test-report.sh; do
  ref=$(block "$dn/$s.md" | sed 's/{Solution}\.slnx/Sample.slnx/' | md5sum)
  while IFS= read -r copy; do
    [ "$(md5sum < "$copy")" = "$ref" ] || err "$copy differs from $dn/$s.md"
  done < <(git ls-files -co --exclude-standard "skills/dotnet/**/example/scripts/$s")
done

[ $fail -eq 0 ] && echo "skills/testing: all checks passed"
exit $fail
