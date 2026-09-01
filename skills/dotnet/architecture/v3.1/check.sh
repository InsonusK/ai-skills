#!/usr/bin/env bash
# v3.1 catalog mechanical checks — see INVARIANTS.md.
# Usage: bash check.sh   (exits non-zero on any hard failure; coverage gaps are warnings)
set -uo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
V31="$REPO/skills/dotnet/architecture/v3.1"
SOL="$V31/solutions"
fail=0
note() { printf '  %s\n' "$1"; }
section() { printf '\n== %s ==\n' "$1"; }

# Solutions named in INVARIANTS.md that may not exist yet mid-build — a link to one of these is a warning, not a failure.
PLANNED='solution-domain-behaviour solution-api-project solution-http-api-publication solution-grpc-integration
solution-infrastructure-project solution-domain-configuration solution-repository-integration solution-unit-of-work
solution-query-integration solution-value-objects solution-domain-rules solution-cecil-architecture-tests
solution-entity-concurrency-change solution-external-created-entity solution-entity-edit-timestamp solution-entity-classification
solution-http-api-client solution-grpc-client solution-messaging-infrastructure solution-kafka-consumer
solution-kafka-producer solution-transactional-outbox'
PLANNED=" $(printf '%s' "$PLANNED" | tr -s '[:space:]' ' ') "
is_planned() { case "$PLANNED" in *" $1 "*) return 0;; *) return 1;; esac; }

resolve() { # $1 = link path relative to repo root (no #fragment); prints nothing, returns 0 if resolvable
  local p="$REPO/$1"
  [ -e "$p" ] && return 0
  [ -e "$p.md" ] && return 0
  case "$1" in
    *.skill) [ -e "$p.md" ] && return 0 ;;              # foo.skill -> foo.skill.md (already tried)
  esac
  return 1
}

section "1. No stale v3 / draft links inside v3.1/solutions"
if grep -rn -E 'architecture/(v3|draft)/' "$SOL" 2>/dev/null | grep -v '/v3\.1/' > /tmp/v31_stale.txt; then
  fail=1; note "stale links:"; sed 's/^/    /' /tmp/v31_stale.txt
else note "ok"; fi

section "2a. Forbidden skill-design constructs in main solution skill files (HARD)"
if grep -rn -E '^#+[[:space:]]*(MUST NOT|SHOULD NOT)([[:space:]]|:|$)|^#[[:space:]]*Anti-patterns' "$SOL"/*/*.skill.md 2>/dev/null > /tmp/v31_forbidden_main.txt; then
  fail=1; note "forbidden headings in main skill files:"; sed 's/^/    /' /tmp/v31_forbidden_main.txt
else note "ok"; fi

section "2b. Same constructs in Implementation/ files (WARNING — tracked debt in DECISIONS.md)"
n=$(grep -rn -E '^#+[[:space:]]*(MUST NOT|SHOULD NOT)([[:space:]]|:|$)|^#[[:space:]]*Anti-patterns' "$SOL"/*/Implementation 2>/dev/null | wc -l)
note "$n occurrences across Implementation files (mechanical pass pending)"

section "3. Absolute wikilink targets resolve (fragments ignored)"
: > /tmp/v31_links.txt ; : > /tmp/v31_links_planned.txt
grep -rhoE '\[\[skills/[^]|#]+' "$SOL" 2>/dev/null | sed 's/^\[\[//' | sort -u | while read -r lnk; do
  resolve "$lnk" && continue
  sname="$(printf '%s' "$lnk" | grep -oE 'solution-[a-z0-9-]+' | head -1)"
  if [ -n "$sname" ] && is_planned "$sname"; then echo "    planned: $lnk" >> /tmp/v31_links_planned.txt
  else echo "    MISSING: $lnk" >> /tmp/v31_links.txt; fi
done
[ -s /tmp/v31_links_planned.txt ] && { note "forward refs to planned solutions (ok mid-build):"; sort -u /tmp/v31_links_planned.txt; }
if [ -s /tmp/v31_links.txt ]; then fail=1; note "unresolved (HARD):"; sort -u /tmp/v31_links.txt; else note "ok"; fi

section "4. Folder / file / name triple matches"
tmiss=0
for d in "$SOL"/solution-*.skill; do
  [ -d "$d" ] || continue
  b="$(basename "$d" .skill)"; f="$d/$b.skill.md"
  if [ ! -f "$f" ]; then fail=1; tmiss=1; note "missing main file for $b"; continue; fi
  nm="$(grep -m1 '^name:' "$f" | sed 's/^name:[[:space:]]*//' | tr -d '"')"
  [ "$nm" = "$b" ] || { fail=1; tmiss=1; note "name mismatch $b: name=$nm"; }
done
[ "$tmiss" -eq 0 ] && note "ok"

section "5. built_on_plateau empty in every v3.1 solution"
if grep -rn -E '^built_on_plateau:[[:space:]]*[^[:space:]]' "$SOL"/*/*.skill.md 2>/dev/null > /tmp/v31_bop.txt; then
  fail=1; note "non-empty:"; sed 's/^/    /' /tmp/v31_bop.txt
else note "ok"; fi

section "6. depends_on targets exist (v3.1-solution entries only), planned ones warned not failed"
: > /tmp/v31_deps.txt ; : > /tmp/v31_deps_planned.txt
awk '/^depends_on:/{d=1;next} /^[a-z_]+:/{d=0} d' "$SOL"/*/*.skill.md 2>/dev/null \
  | grep -oE 'architecture/v3\.1/solutions/solution-[a-z0-9-]+\.skill' | sed 's#.*/##' | sort -u | while read -r sname; do
  [ -d "$V31/solutions/$sname" ] && continue
  if is_planned "${sname%.skill}"; then echo "    planned: $sname" >> /tmp/v31_deps_planned.txt
  else echo "    MISSING: $sname" >> /tmp/v31_deps.txt; fi
done
[ -s /tmp/v31_deps_planned.txt ] && { note "depends_on planned solutions (ok mid-build):"; sort -u /tmp/v31_deps_planned.txt; }
if [ -s /tmp/v31_deps.txt ]; then fail=1; note "unresolved depends_on (HARD):"; sort -u /tmp/v31_deps.txt; else note "ok"; fi

section "7. Feature / VP coverage (WARNING only — expected incomplete mid-build)"
: > /tmp/v31_cov.txt
for s in central-package-management soft-value-objects validation-behavior mediator-exception-handler \
         dto-property-validators mediator-integration app-logging dotnet-conformance-testing sln-structure \
         domain-behaviour infrastructure-project domain-configuration repository-integration unit-of-work \
         query-integration value-objects domain-rules cecil-architecture-tests entity-concurrency-change \
         external-created-entity entity-edit-timestamp entity-classification api-project \
         http-api-publication grpc-integration; do
  [ -d "$SOL/solution-$s.skill" ] || echo "    not yet: solution-$s" >> /tmp/v31_cov.txt
done
if [ -s /tmp/v31_cov.txt ]; then cat /tmp/v31_cov.txt; else note "ok — all non-aspirational solutions present"; fi

section "RESULT"
if [ "$fail" -eq 0 ]; then echo "PASS"; exit 0; else echo "FAIL"; exit 1; fi
