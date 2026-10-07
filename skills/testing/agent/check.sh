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
out=$(awk -F'\t' '$1 ~ /^skills\/testing\// && $1 !~ /-in-[a-z]+\.skill/ && $4 ~ /-in-[a-z]+\.skill/ { print $1 "\t" $4 }' "$links" | sort -u)
[ -n "$out" ] && { err "stack-agnostic skill links a stack-specialized skill:"; echo "$out"; }

# 4. Layout: skills/testing/{topic}/ holds only {topic}.skill[.md] and {topic}-in-{stack}.skill[.md];
#    the skill file's name: matches; a stack skill carries its stack/{stack} tag, an agnostic one the bare stack tag.
stacks='angular|dotnet|go|python|typescript'
for d in skills/testing/*/; do
  topic=$(basename "$d"); [ "$topic" = agent ] && continue
  for e in "$d"*; do
    b=$(basename "$e"); n=${b%.md}; n=${n%.skill}
    [[ "$b" == *.skill || "$b" == *.skill.md ]] || { err "$e: not a skill"; continue; }
    [[ "$n" == "$topic" || "$n" =~ ^$topic-in-($stacks)$ ]] || { err "$e: name is neither $topic nor $topic-in-{stack}"; continue; }
    f=$e; [ -d "$e" ] && f="$e/$n.skill.md"
    [ -f "$f" ] || { err "$f: missing skill file"; continue; }
    grep -qx "name: $n" "$f" || err "$f: frontmatter name is not $n"
    if [[ "$n" =~ -in-($stacks)$ ]]; then
      want="stack/${BASH_REMATCH[1]}"; [ "${BASH_REMATCH[1]}" = angular ] && want="framework/angular"   # angular is a framework on stack/typescript
      grep -qE "^\s*- $want$" "$f" || err "$f: missing tag $want"
    else
      grep -qE "^\s*- stack$" "$f" || err "$f: missing bare stack tag"
    fi
  done
done

# 5. No reference to the old locations or the old names.
out=$(git grep -nIE 'skills/(common-workflow|angular|dotnet|go|python|typescript)/test/[a-z]|cucmber|no-test-theater-(angular|dotnet|python)\b|\bdotnet-unittest\b' \
      -- . ':!.validation' ":!$A" ':!*/agent/DECISIONS.md' ':!*/adr/*' | cut -c1-200)
[ -n "$out" ] && { err "references to the old test locations/names:"; echo "$out"; }

[ $fail -eq 0 ] && echo "skills/testing: all checks passed"
exit $fail
