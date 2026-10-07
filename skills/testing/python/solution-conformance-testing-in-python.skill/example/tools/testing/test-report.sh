#!/usr/bin/env bash
# Builds the report directory from what the test kinds left in the work directory:
# native reports copied as they are, badges and the scenario page computed from the
# normalized result/*.json files. Called by `make test-report`, which exports:
#   TEST_WORK_DIR     kinds/<kind>/{result,report}/ live below it
#   TEST_REPORT_DIR   emptied by make before this runs; filled here
set -euo pipefail

WORK_DIR="${TEST_WORK_DIR:-tmp/testing}"
REPORT_DIR="${TEST_REPORT_DIR:-$WORK_DIR/report}"

mkdir -p "$REPORT_DIR/reports" "$REPORT_DIR/badges"
cp report-template/index.html "$REPORT_DIR/index.html"

# Every kinds/<kind>/report/<name>/ becomes reports/<name>/, so a new kind's report is
# published without changing this script.
for src in "$WORK_DIR"/kinds/*/report/*/; do
  [ -d "$src" ] || continue
  name=$(basename "$src")
  if [ -e "$REPORT_DIR/reports/$name" ]; then
    echo "test-report: two test kinds wrote a report named '$name'" >&2
    exit 1
  fi
  cp -r "$src" "$REPORT_DIR/reports/$name"
done

# Path of the normalized result file of that name, from whichever kind wrote it; empty
# when no kind did (a check run reports no coverage, a skipped kind leaves no result).
result() {
  local f
  for f in "$WORK_DIR"/kinds/*/result/"$1"; do
    if [ -f "$f" ]; then echo "$f"; return 0; fi
  done
  return 0
}

score_color() {
  awk -v s="$1" 'BEGIN { if (s >= 80) print "brightgreen"; else if (s >= 60) print "yellowgreen"; else print "red" }'
}

UNIT=$(result unit-test.json)
if [ -n "$UNIT" ]; then
  TOTAL=$(jq '.total' "$UNIT")
  PASSED=$(jq '.passed' "$UNIT")
  COLOR="red"
  [ "$PASSED" = "$TOTAL" ] && COLOR="brightgreen"
  printf '{"schemaVersion":1,"label":"tests","message":"%s/%s passed","color":"%s"}' \
    "$PASSED" "$TOTAL" "$COLOR" > "$REPORT_DIR/badges/tests.json"
fi

COVERAGE=$(result coverage-test.json)
if [ -n "$COVERAGE" ]; then
  PCT=$(jq '.linePct' "$COVERAGE")
  printf '{"schemaVersion":1,"label":"coverage","message":"%s%%","color":"%s"}' \
    "$PCT" "$(score_color "$PCT")" > "$REPORT_DIR/badges/coverage.json"
fi

MUTATION=$(result mutation-test.json)
if [ -n "$MUTATION" ]; then
  SCORE=$(jq '.score' "$MUTATION")
  printf '{"schemaVersion":1,"label":"mutation score","message":"%s%%","color":"%s"}' \
    "$SCORE" "$(score_color "$SCORE")" > "$REPORT_DIR/badges/mutation.json"
fi

# scenarios.json -> reports/scenarios/index.html: type x status table, then every entry
# grouped by feature. "attention" marks untyped, missing, failed, and todo
# happy/negative/error entries without a note.
SCENARIOS=$(result scenarios.json)
if [ -n "$SCENARIOS" ]; then
  mkdir -p "$REPORT_DIR/reports/scenarios"
  jq -r '
    ["happy","boundary","negative","error","concurrency","security","regression","untyped"] as $types
    | ["passed","failed","todo","missing"] as $statuses
    | .scenarios as $all
    | def attention: .type == "untyped" or .status == "missing" or .status == "failed"
        or (.status == "todo" and .note == "" and (.type == "happy" or .type == "negative" or .type == "error"));
    "<!doctype html><html><head><meta charset=\"utf-8\"><title>Scenarios</title>",
    "<style>td,th{border:1px solid #999;padding:2px 6px}table{border-collapse:collapse}.attention{background:#fdd}</style></head><body>",
    "<h1>Scenarios</h1><h2>By type</h2><table><tr><th>type</th>" + ($statuses | map("<th>\(.)</th>") | join("")) + "</tr>",
    ($types[] as $t | "<tr><td>\($t)</td>" + ($statuses | map(. as $s | "<td>\([$all[] | select(.type == $t and .status == $s)] | length)</td>") | join("")) + "</tr>"),
    "</table>",
    ($all | group_by(.feature)[] |
      "<h2>\(.[0].feature | @html)</h2><table><tr><th>scenario</th><th>examples</th><th>type</th><th>status</th><th>location</th><th>note</th></tr>",
      (.[] | "<tr\(if attention then " class=\"attention\"" else "" end)><td>\(.scenario | @html)</td><td>\(.examples | @html)</td><td>\(.type)</td><td>\(.status)</td><td>\(.uri | @html):\(.line)</td><td>\(.note | @html)</td></tr>"),
      "</table>"),
    "</body></html>"
  ' "$SCENARIOS" > "$REPORT_DIR/reports/scenarios/index.html"
fi

# A report whose tool wrote no entry page gets one listing what it holds, so reports/<name>/
# - where the landing page and a README badge point - opens on a static host too.
for dir in "$REPORT_DIR"/reports/*/; do
  [ -d "$dir" ] && [ ! -f "${dir}index.html" ] || continue
  entries=("$dir"*)   # listed before the page itself exists
  { printf '<!doctype html><html><head><meta charset="utf-8"><title>%s</title></head><body><h1>%s</h1><ul>\n' "$(basename "$dir")" "$(basename "$dir")"
    for entry in "${entries[@]}"; do
      name=$(basename "$entry")
      if   [ -f "$entry" ];            then printf '<li><a href="%s">%s</a></li>\n' "$name" "$name"
      elif [ -f "$entry/index.html" ]; then printf '<li><a href="%s/">%s/</a></li>\n' "$name" "$name"
      else
        for file in "$entry"/*; do
          [ -f "$file" ] && printf '<li><a href="%s">%s</a></li>\n' "$name/$(basename "$file")" "$name/$(basename "$file")"
        done
      fi
    done
    printf '</ul></body></html>\n'; } > "${dir}index.html"
done
