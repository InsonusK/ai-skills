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
      [[ "$n" == *-in-$dir || ( "$dir" = angular && "$n" =~ ^solution-conformance-testing-in-angular-(library|nx)$ ) ]] || err "$e: name does not end with -in-$dir"
      want="stack/$dir"; [ "$dir" = angular ] && want="framework/angular"   # angular is a framework on stack/typescript
      grep -qE "^\s*- $want$" "$f" || err "$f: missing tag $want"
    fi
  done
done

# 5. No reference to the old locations or the old names.
out=$(git grep -nIE 'skills/(common-workflow|angular|dotnet|go|python|typescript)/test/[a-z]|cucmber|no-test-theater-(angular|dotnet|python)\b|\bdotnet-unittest\b' \
      -- . ':!.validation' ":!$A" ':!*/agent/DECISIONS.md' ':!*/adr/*' | cut -c1-200)
[ -n "$out" ] && { err "references to the old test locations/names:"; echo "$out"; }

# 6. Shared files in every example are verbatim copies of the core skill's assets.
core=skills/testing/core/solution-conformance-testing.skill/assets
for f in tools/testing/testing.mk tools/testing/testing.sh tools/testing/kind.sh tools/testing/test-report.sh \
         tools/testing/normalize-scenarios.sh tools/testing/messages-results.jq \
         tools/livingdoc/render.mjs tools/livingdoc/package.json tools/livingdoc/package-lock.json; do
  while IFS= read -r copy; do
    cmp -s "$core/$f" "$copy" || err "$copy differs from $core/$f"
  done < <(git ls-files -co --exclude-standard "skills/**/examples/$f")
done

# 7. Every example on the contract: its Makefile parses, lists kinds, its README shows every declared badge,
#    and its .gitignore covers the default work directory.
while IFS= read -r mk; do
  dir=$(dirname "$mk")
  kinds=$(make -s -C "$dir" test-kinds 2>/dev/null) || { err "$dir: make test-kinds fails"; continue; }
  [ -n "$kinds" ] || err "$dir: make test-kinds prints nothing"
  make -s -C "$dir" test-readme-check >/dev/null 2>&1 || err "$dir: make test-readme-check fails"
  grep -qE '^/?tmp/?$' "$dir/.gitignore" 2>/dev/null || err "$dir/.gitignore does not ignore tmp/"
  grep -qE '^/?tools/livingdoc/node_modules/?$' "$dir/.gitignore" 2>/dev/null || err "$dir/.gitignore does not ignore tools/livingdoc/node_modules/"
  grep -q '<!-- test-reports -->' "$dir/report-template/index.html" 2>/dev/null || err "$dir/report-template/index.html has no <!-- test-reports --> line"
  for k in $(echo "$kinds" | cut -d' ' -f1); do
    make -n -C "$dir" "test-kind-$k" >/dev/null 2>&1 || err "$dir: no target test-kind-$k"
  done
done < <(git grep -l 'include tools/testing/testing.mk' -- 'skills/**/examples/Makefile')

# 8. No fenced script left in a templates/ description: code is a real file under assets/ or templates/.
out=$(grep -lE '^```(bash|makefile|go|js|jq|json|html)$' skills/testing/*/*.skill/templates/*.md skills/testing/*/*.skill/Implementation/tools/*/*.md skills/testing/*/*.skill/Implementation/tools/*/*/*.md 2>/dev/null)
[ -n "$out" ] && { err "code still inline instead of a real file:"; echo "$out"; }

# 9. No caller-facing remnant of the previous contract.
out=$(git grep -nE 'make unit-test|make mutation-test|test-report-build|TEST_BADGES_|ONLY_DELTA|WITH_CODE_COVERAGE \?=|-badge\.json|tmp/result/|public/(tests|coverage|mutation|scenarios)' \
      -- skills ':!*/adr/*' ':!*/agent/*' ':!*/recheck/*' | cut -c1-200)
[ -n "$out" ] && { err "references to the previous testing contract:"; echo "$out"; }

# 10. Example tooling equals the stack skill's assets: the kind scripts, and the Go normalizers they call.
for s in go dotnet; do
  src=skills/testing/$s/solution-conformance-testing-in-$s.skill/assets
  while IFS= read -r copy; do
    cmp -s "$src/tools/testing/kinds/$(basename "$copy")" "$copy" || err "$copy differs from $src/tools/testing/kinds/"
  done < <(git ls-files -co --exclude-standard "skills/$s/**/examples/tools/testing/kinds/*.sh")
done
for s in go python typescript dotnet; do   # the runnable example inside the testing skill itself
  src=skills/testing/$s/solution-conformance-testing-in-$s.skill
  while IFS= read -r copy; do
    cmp -s "$src/assets/tools/testing/kinds/$(basename "$copy")" "$copy" || err "$copy differs from $src/assets/tools/testing/kinds/"
  done < <(git ls-files -co --exclude-standard "$src/examples/tools/testing/kinds/*")
done
ng=skills/testing/angular/solution-conformance-testing-in-angular.skill; tsk=skills/testing/typescript/solution-conformance-testing-in-typescript.skill/assets/tools/testing/kinds
while IFS= read -r copy; do   # the Angular example: its own two kinds, the TypeScript parent's two unchanged
  k=$(basename "$copy"); src=$ng/assets/tools/testing/kinds/$k; [ -f "$src" ] || src=$tsk/$k
  cmp -s "$src" "$copy" || err "$copy differs from $src"
done < <(git ls-files -co --exclude-standard "$ng/examples/tools/testing/kinds/*")
for shape in library nx; do
  refinement=skills/testing/angular/solution-conformance-testing-in-angular-$shape.skill
  [ -d "$refinement" ] || continue
  while IFS= read -r copy; do
    k=$(basename "$copy"); src=$refinement/assets/tools/testing/kinds/$k
    [ -f "$src" ] || src=$ng/assets/tools/testing/kinds/$k
    [ -f "$src" ] || src=$tsk/$k
    cmp -s "$src" "$copy" || err "$copy differs from $src"
  done < <(git ls-files -co --exclude-standard "$refinement/examples/tools/testing/kinds/*")
  while IFS= read -r asset; do
    relative=${asset#"$refinement/assets/"}
    cmp -s "$asset" "$refinement/examples/$relative" || err "$refinement/examples/$relative differs from $asset"
  done < <(git ls-files -co --exclude-standard "$refinement/assets/*")
  cmp -s "$ng/assets/tools/testing/angular-results.mjs" "$refinement/examples/tools/testing/angular-results.mjs" || err "$refinement: changed inherited Angular result adapter"
done
ts=skills/testing/typescript/solution-conformance-testing-in-typescript.skill
for f in cucumber.mjs stryker.conf.json; do
  cmp -s "$ts/assets/$f" "$ts/examples/$f" || err "$ts/examples/$f differs from $ts/assets/$f"
done
go=skills/testing/go/solution-conformance-testing-in-go.skill/assets
while IFS= read -r copy; do
  cmp -s "$go/tools/$(basename "$(dirname "$copy")")/main.go" "$copy" || err "$copy differs from $go"
done < <(git ls-files -co --exclude-standard 'skills/go/**/examples/tools/normalize_*/main.go' 'skills/testing/go/**/examples/tools/normalize_*/main.go')

# 11. One Makefile, one report builder: no testing recipe in an example Makefile, no stack-own report builder.
out=$(git grep -lE '^test-(kind|report|and-report)[a-z-]*:' -- 'skills/**/examples/Makefile'; git ls-files 'skills/**/tools/test_report/*' 'skills/**/examples/scripts/test-report.sh')
[ -n "$out" ] && { err "testing recipe or report builder outside tools/testing/:"; echo "$out"; }

# 12. The removed page must not come back as a live function/call.
out=$(git grep -n 'kind_scenarios_report' -- skills ':!*/agent/*' ':!*/adr/*')
[ -n "$out" ] && { err "removed scenario-page function/call:"; echo "$out"; }

[ $fail -eq 0 ] && echo "skills/testing: all checks passed"
exit $fail
