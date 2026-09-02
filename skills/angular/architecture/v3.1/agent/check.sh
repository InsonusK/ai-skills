#!/usr/bin/env bash
# Angular v3.1 catalog mechanical checks — see agent/INVARIANTS.md.
# Usage: bash skills/angular/architecture/v3.1/agent/check.sh
set -uo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../../.." && pwd)"
V31="$REPO/skills/angular/architecture/v3.1"
SOL="$V31/solutions"
fail=0
note() { printf '  %s\n' "$1"; }
section() { printf '\n== %s ==\n' "$1"; }

# Aspirational / not-yet-authored solutions: a link to one is a warning, not a failure.
PLANNED='solution-persisted-state solution-design-system-multi-tenant-theming
plateau-async-monolith plateau-offline-read-monolith plateau-offline-full-monolith plateau-multiuser-monolith
plateau-design-system plateau-platform-host plateau-embeddable-app'
PLANNED=" $(printf '%s' "$PLANNED" | tr -s '[:space:]' ' ') "
is_planned() { case "$PLANNED" in *" $1 "*) return 0;; *) return 1;; esac; }

resolve() { local p="$REPO/$1"; [ -e "$p" ] && return 0; [ -e "$p.md" ] && return 0; return 1; }

section "1. No stale V1-catalog links inside v3.1/"
if grep -rn -E 'skills/angular/architecture/(solutions|plateau)/' "$V31" 2>/dev/null | grep -v '/v3\.1/' > /tmp/ng31_stale.txt; then
  fail=1; note "stale V1 links:"; sed 's/^/    /' /tmp/ng31_stale.txt
else note "ok"; fi

section "2. No Cyrillic outside solution-ui-testing glossary (tracked debt)"
n=$(grep -rlIP '[\x{0400}-\x{04FF}]' "$V31" 2>/dev/null | grep -vE 'solution-ui-testing.skill/glossary/|/example/' | wc -l)
if [ "$n" -gt 0 ]; then fail=1; note "Cyrillic in $n non-glossary files:"; grep -rlIP '[\x{0400}-\x{04FF}]' "$V31" | grep -vE 'solution-ui-testing.skill/glossary/|/example/' | sed 's/^/    /'
else note "ok (glossary translation is tracked debt)"; fi

section "3a. Forbidden skill-design headings in main solution files (HARD)"
if grep -rn -E '^#+[[:space:]]*(MUST NOT|SHOULD NOT)([[:space:]]|:|$)|^#[[:space:]]*Anti-patterns' "$SOL"/*/*.skill.md 2>/dev/null > /tmp/ng31_fm.txt; then
  fail=1; note "forbidden headings:"; sed 's/^/    /' /tmp/ng31_fm.txt
else note "ok"; fi

section "3b. Same in Implementation/ + adr/ (WARNING — tracked debt)"
n=$(grep -rn -E '^#+[[:space:]]*(MUST NOT|SHOULD NOT)([[:space:]]|:|$)|^#[[:space:]]*Anti-patterns' "$SOL"/*/Implementation "$SOL"/*/adr 2>/dev/null | wc -l)
note "$n occurrences"

section "4. triggers: replaced by whenToUse: in every main file (HARD)"
tmiss=0
for f in "$SOL"/*/*.skill.md; do
  grep -q '^whenToUse:' "$f" || { fail=1; tmiss=1; note "no whenToUse: $(basename "$(dirname "$f")")"; }
  grep -q '^triggers:' "$f" && { fail=1; tmiss=1; note "still has triggers: $(basename "$(dirname "$f")")"; }
done
[ "$tmiss" -eq 0 ] && note "ok"

section "5. version 20260902000000 in every main file"
if grep -rL '^version: 20260902000000' "$SOL"/*/*.skill.md > /tmp/ng31_ver.txt 2>/dev/null && [ -s /tmp/ng31_ver.txt ]; then
  fail=1; note "wrong/missing version:"; sed 's/^/    /' /tmp/ng31_ver.txt
else note "ok"; fi

section "6. No built_on_plateau field anywhere (angular V1 never had it)"
if grep -rn '^built_on_plateau:' "$SOL"/*/*.skill.md 2>/dev/null > /tmp/ng31_bop.txt; then
  fail=1; note "found:"; sed 's/^/    /' /tmp/ng31_bop.txt
else note "ok"; fi

section "7. Folder / file / name triple matches"
tmiss=0
for d in "$SOL"/*.skill; do
  b="$(basename "$d" .skill)"; f="$d/$b.skill.md"
  [ -f "$f" ] || { fail=1; tmiss=1; note "missing main file: $b"; continue; }
  nm="$(grep -m1 '^name:' "$f" | sed 's/^name:[[:space:]]*//' | tr -d '"')"
  [ "$nm" = "$b" ] || { fail=1; tmiss=1; note "name mismatch $b: name=$nm"; }
done
[ "$tmiss" -eq 0 ] && note "ok"

section "8. Absolute wikilink targets resolve (fragments ignored)"
: > /tmp/ng31_links.txt ; : > /tmp/ng31_planned.txt
grep -rhoE '\[\[skills/[^]|#]+' "$V31" 2>/dev/null | grep -v '/example/' | sed 's/^\[\[//; s/\\$//' | sort -u | while read -r lnk; do
  resolve "$lnk" && continue
  sname="$(printf '%s' "$lnk" | grep -oE '(solution|plateau)-[a-z0-9-]+' | head -1)"
  if [ -n "$sname" ] && is_planned "$sname"; then echo "    planned: $lnk" >> /tmp/ng31_planned.txt
  else echo "    MISSING: $lnk" >> /tmp/ng31_links.txt; fi
done
[ -s /tmp/ng31_planned.txt ] && { note "forward refs to aspirational solutions (ok):"; sort -u /tmp/ng31_planned.txt; }
if [ -s /tmp/ng31_links.txt ]; then fail=1; note "unresolved (HARD):"; sort -u /tmp/ng31_links.txt; else note "ok"; fi

section "9. depends_on targets exist"
: > /tmp/ng31_deps.txt
awk '/^depends_on:/{d=1;next} /^[a-z_]+:/{d=0} d' "$SOL"/*/*.skill.md 2>/dev/null \
  | grep -oE 'v3\.1/solutions/solution-[a-z0-9-]+\.skill' | sed 's#.*/##' | sort -u | while read -r sname; do
  [ -d "$SOL/$sname" ] || echo "    MISSING: $sname" >> /tmp/ng31_deps.txt
done
if [ -s /tmp/ng31_deps.txt ]; then fail=1; note "unresolved depends_on (HARD):"; sort -u /tmp/ng31_deps.txt; else note "ok"; fi

section "RESULT"
if [ "$fail" -eq 0 ]; then echo "PASS"; exit 0; else echo "FAIL"; exit 1; fi
