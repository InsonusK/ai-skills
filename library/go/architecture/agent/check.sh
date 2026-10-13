#!/usr/bin/env bash
# library/go/architecture catalog mechanical checks — see agent/INVARIANTS.md.
# Usage: bash agent/check.sh   (exits non-zero on any hard failure; coverage gaps are warnings)
set -uo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../.." && pwd)"
CAT="$REPO/library/go/architecture"
SOL="$CAT/solutions"
fail=0
note() { printf '  %s\n' "$1"; }
section() { printf '\n== %s ==\n' "$1"; }

# Solutions named in INVARIANTS.md that are skeletons by design — a link to
# one of these is a warning, not a failure.
PLANNED='solution-go-messaging-infrastructure solution-go-kafka-producer solution-go-kafka-consumer solution-go-transactional-outbox'
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

section "1. No stray links to a non-existent versioned/staging tree"
if grep -rn -E 'architecture/(v[0-9]|draft)/' "$SOL" 2>/dev/null > /tmp/go_stale.txt; then
  fail=1; note "stale links:"; sed 's/^/    /' /tmp/go_stale.txt
else note "ok"; fi

section "2. Forbidden skill-design constructs (HARD)"
if grep -rn -E '^#+[[:space:]]*(MUST NOT|SHOULD NOT)([[:space:]]|:|$)|^#[[:space:]]*Anti-patterns' "$SOL"/*/*.skill.md "$SOL"/*/Implementation -r 2>/dev/null > /tmp/go_forbidden.txt; then
  fail=1; note "forbidden headings:"; sed 's/^/    /' /tmp/go_forbidden.txt
else note "ok"; fi

section "3. Absolute wikilink targets resolve (fragments ignored)"
: > /tmp/go_links.txt ; : > /tmp/go_links_planned.txt
grep -rhoE '\[\[library/[^]|#]+' "$SOL" 2>/dev/null | sed 's/^\[\[//' | sort -u | while read -r lnk; do
  resolve "$lnk" && continue
  sname="$(printf '%s' "$lnk" | grep -oE 'solution-[a-z0-9-]+' | head -1)"
  if [ -n "$sname" ] && is_planned "$sname"; then echo "    planned: $lnk" >> /tmp/go_links_planned.txt
  else echo "    MISSING: $lnk" >> /tmp/go_links.txt; fi
done
[ -s /tmp/go_links_planned.txt ] && { note "forward refs to skeleton solutions (ok, by design):"; sort -u /tmp/go_links_planned.txt; }
if [ -s /tmp/go_links.txt ]; then fail=1; note "unresolved (HARD):"; sort -u /tmp/go_links.txt; else note "ok"; fi

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

section "5. built_on_plateau empty in every catalog solution"
if grep -rn -E '^built_on_plateau:[[:space:]]*[^[:space:]]' "$SOL"/*/*.skill.md 2>/dev/null > /tmp/go_bop.txt; then
  fail=1; note "non-empty:"; sed 's/^/    /' /tmp/go_bop.txt
else note "ok"; fi

section "6. depends_on targets exist (catalog-solution entries only), skeletons warned not failed"
: > /tmp/go_deps.txt ; : > /tmp/go_deps_planned.txt
awk '/^depends_on:/{d=1;next} /^[a-z_]+:/{d=0} d' "$SOL"/*/*.skill.md 2>/dev/null \
  | grep -oE 'library/[a-z-]+/architecture/solutions/solution-[a-z0-9-]+\.skill' | sort -u | while read -r spath; do
  # A stack-agnostic base (library/common-workflow/architecture/solutions/) is a legal depends_on target.
  [ -d "$REPO/$spath" ] && continue
  sname="${spath##*/}"
  if is_planned "${sname%.skill}"; then echo "    planned: $sname" >> /tmp/go_deps_planned.txt
  else echo "    MISSING: $sname" >> /tmp/go_deps.txt; fi
done
[ -s /tmp/go_deps_planned.txt ] && { note "depends_on skeleton solutions (ok, by design):"; sort -u /tmp/go_deps_planned.txt; }
if [ -s /tmp/go_deps.txt ]; then fail=1; note "unresolved depends_on (HARD):"; sort -u /tmp/go_deps.txt; else note "ok"; fi

section "7. Feature / VP coverage (WARNING only — expected incomplete mid-build)"
: > /tmp/go_cov.txt
for s in go-repository-structure go-domain-ports go-domain-logic go-http-api go-app-logging \
         go-conformance-testing grpc-api external-integration cached-db persistent-db \
         go-messaging-infrastructure go-kafka-producer go-kafka-consumer go-transactional-outbox; do
  [ -d "$SOL/solution-$s.skill" ] || echo "    not yet: solution-$s" >> /tmp/go_cov.txt
done
if [ -s /tmp/go_cov.txt ]; then cat /tmp/go_cov.txt; else note "ok — all 14 catalog solutions present"; fi

section "8. Plateau skills — relative links resolve, forbidden headings, name triple"
PLA="$CAT/plateau"
if [ -d "$PLA" ]; then
  # 8a. forbidden ## MUST NOT / ## SHOULD NOT / # Anti-patterns in plateau skill files (HARD)
  if grep -rn -E '^#+[[:space:]]*(MUST NOT|SHOULD NOT)([[:space:]]|:|$)|^#[[:space:]]*Anti-patterns' \
       "$PLA"/*/structure "$PLA"/*/*.skill/*.skill.md 2>/dev/null > /tmp/go_pla_forbidden.txt; then
    fail=1; note "forbidden headings in plateau skill files:"; sed 's/^/    /' /tmp/go_pla_forbidden.txt
  else note "8a forbidden headings: ok"; fi

  # 8b. every markdown link target (relative) inside a plateau skill file resolves
  : > /tmp/go_pla_links.txt
  while IFS= read -r f; do
    d="$(dirname "$f")"
    grep -oE '\]\(\.\.?[^)]*\)' "$f" 2>/dev/null | sed -E 's/^\]\(//; s/\)$//; s/#.*$//' | while read -r rel; do
      [ -z "$rel" ] && continue
      case "$rel" in http*) continue;; esac
      [ -e "$d/$rel" ] || echo "    MISSING: $f -> $rel" >> /tmp/go_pla_links.txt
    done
    grep -oE '\[\[[^]|]+' "$f" 2>/dev/null | sed 's/^\[\[//; s/#.*$//; s/\\$//' | while read -r wl; do
      case "$wl" in
        library/*) resolve "$wl" || { sn="$(printf '%s' "$wl" | grep -oE 'solution-[a-z0-9-]+' | head -1)"; \
                   { [ -n "$sn" ] && is_planned "$sn"; } || echo "    MISSING(abs): $f -> $wl" >> /tmp/go_pla_links.txt; } ;;
        ../*|./*) [ -e "$d/$wl" ] || [ -e "$d/$wl.md" ] || echo "    MISSING(rel): $f -> $wl" >> /tmp/go_pla_links.txt ;;
      esac
    done
  done < <(find "$PLA" -name '*.skill.md')
  if [ -s /tmp/go_pla_links.txt ]; then fail=1; note "unresolved plateau links (HARD):"; sort -u /tmp/go_pla_links.txt
  else note "8b plateau links: ok"; fi

  # 8c. plateau element skill: file basename == name: frontmatter, and starts with plateau-<plateau>--
  pmiss=0
  while IFS= read -r f; do
    b="$(basename "$f" .skill.md)"
    case "$b" in plateau-*--*) ;; *) continue;; esac
    nm="$(grep -m1 '^name:' "$f" | sed 's/^name:[[:space:]]*//' | tr -d '"'"'"'')"
    [ "$nm" = "$b" ] || { fail=1; pmiss=1; note "plateau name mismatch: $b (name: $nm)"; }
  done < <(find "$PLA" -name 'plateau-*--*.skill.md')
  [ "$pmiss" -eq 0 ] && note "8c plateau name triple: ok"
else
  note "no plateau/ folder yet"
fi

section "9. Every Implementation file with element_kind: struct|functions|package|repository carries a matching element/* tag"
tmiss=0
while IFS= read -r f; do
  ek="$(grep -m1 '^element_kind:' "$f" | sed 's/^element_kind:[[:space:]]*//')"
  case "$ek" in struct|functions|package|repository) ;; *) continue;; esac
  grep -q '^\s*-\s*element/' "$f" || { fail=1; tmiss=1; note "missing element/* tag: $f"; }
done < <(find "$SOL" -path '*/Implementation/*' -name '*.md')
[ "$tmiss" -eq 0 ] && note "ok"

section "10. API contracts (INVARIANTS.md §9, issue #206)"
amiss=0
# 10a. No statement of the previous layout or wire form outside the records of how it was.
if git -C "$CAT" grep -nE 'proto/\{|proto/linkcheck|proto/reputation|buf/buf\.gen|buf/\{|buf/reputation|gen/reputation|gen/\{external\}|StatusBadGateway|go install [^ ]*protoc-gen|map\[string\]string\{"error"' \
     -- solutions plateau feature variability-map.md ':!*/adr/*' ':!*/examples/go.sum' ':!*/examples/internal/taskbox/*' 2>/dev/null \
     | grep -vE 'Violation:|at the time|before 2026-10-12|\(it was ' > /tmp/go_api_old.txt; then
  fail=1; amiss=1; note "previous contract layout or wire form still stated:"; sed 's/^/    /' /tmp/go_api_old.txt | cut -c1-220
fi
# 10b. Every plateau example carries the contract, the docs switch, the feature, and no old folder.
for ex in "$CAT"/plateau/*/*.skill/examples; do
  rel="${ex#$CAT/}"
  ls "$ex"/docs/integration/api/*/embed.go >/dev/null 2>&1 || { fail=1; amiss=1; note "$rel: no docs/integration/api/{contract}/embed.go"; }
  for d in proto buf; do [ -e "$ex/$d" ] && { fail=1; amiss=1; note "$rel: $d/ still exists"; }; done
  grep -q 'HTTP_API_DOCS_ENABLED' "$ex/internal/config/config.go" && grep -q 'HTTP_BASE_PATH' "$ex/internal/config/config.go" \
    || { fail=1; amiss=1; note "$rel: config.go lacks HTTP_API_DOCS_ENABLED or HTTP_BASE_PATH"; }
  grep -qs 'both have exactly the operations' "$ex"/internal/api/http/features/*.feature || { fail=1; amiss=1; note "$rel: no operations scenario in the HTTP API feature"; }
  grep -qs 'contractRoutes()' "$ex/internal/api/http/server.go" || { fail=1; amiss=1; note "$rel: server.go has no contractRoutes()"; }
  if ls "$ex"/docs/integration/api/*/*.proto >/dev/null 2>&1; then
    for f in buf.yaml buf.lock buf.gen.yaml tools/testing/kinds/generated.sh; do [ -f "$ex/$f" ] || { fail=1; amiss=1; note "$rel: proto contract without $f"; }; done
    ls "$ex"/docs/integration/api/*/openapi.json >/dev/null 2>&1 && { fail=1; amiss=1; note "$rel: hand-written openapi.json beside a proto contract"; }
    head -1 "$ex"/docs/integration/api/*/*.proto | grep -qE '^// version: [0-9]+\.[0-9]+\.[0-9]+$' || { fail=1; amiss=1; note "$rel: proto file does not start with // version: x.y.z"; }
    grep -q 'tool (' "$ex/go.mod" || grep -q '^tool ' "$ex/go.mod" || { fail=1; amiss=1; note "$rel: go.mod has no tool lines"; }
  else
    [ -f "$ex/tools/testing/kinds/generated.sh" ] && { fail=1; amiss=1; note "$rel: generated kind without a proto contract"; }
  fi
  if [ -d "$ex/docs/integration/client" ]; then [ -f "$ex/buf.gen.client.yaml" ] || { fail=1; amiss=1; note "$rel: consumed contract without buf.gen.client.yaml"; }; fi
done
[ "$amiss" -eq 0 ] && note "ok"

section "RESULT"
if [ "$fail" -eq 0 ]; then echo "PASS"; exit 0; else echo "FAIL"; exit 1; fi
