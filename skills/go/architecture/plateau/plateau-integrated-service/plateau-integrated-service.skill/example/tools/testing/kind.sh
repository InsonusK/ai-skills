# tools/testing/kind.sh - sourced by every tools/testing/kinds/<kind>.sh (solution-conformance-testing).
# Copied verbatim into every project; never edited there.
#
# A kind script gets from the environment:
#   TEST_KIND_DIR      the only directory it may write to; result/, report/ and badges/ exist
#                      and are empty
#   TEST_RUN_PURPOSE   check | report        DELTA_BASE   a ref, or empty
# It decides everything about its own output: result/ holds its data, report/<name>/ what a
# person opens, badges/<name>.json the badge of that report - written through the kind_badge
# functions below. It says what the purpose changed through kind_mode (or leaves through
# kind_skip) and exits with its tool's own exit code. tools/testing/test-report.sh only
# gathers what the kinds left.

: "${TEST_KIND_DIR:?run this through make test-kind-<kind>}"
RESULT_DIR="$TEST_KIND_DIR/result"
REPORT_DIR="$TEST_KIND_DIR/report"
BADGE_DIR="$TEST_KIND_DIR/badges"

# kind_mode <text> - what the kind does because of the run's purpose: to the log and the report.
kind_mode() {
  echo "test-kind-$TEST_KIND [purpose $TEST_RUN_PURPOSE]: $1"
  echo "$1" > "$TEST_KIND_DIR/mode"
}

# kind_skip <reason> - the kind does not apply to this run: no result, no badge, exit 0.
kind_skip() {
  echo "test-kind-$TEST_KIND [purpose $TEST_RUN_PURPOSE]: skipped - $1"
  echo "$1" > "$TEST_KIND_DIR/skipped"
  exit 0
}

# kind_livingdoc - render report/tests/cucumber/ (the runner's standard Cucumber report) into
# report/tests/livingdoc/ with the shared tools/livingdoc, and make it what report/tests/
# opens when the runner's own report has no entry page. Never fails the kind; skipped
# without npm.
kind_livingdoc() {
  if command -v npm >/dev/null 2>&1; then
    { npm ci --prefix tools/livingdoc --silent \
        && node tools/livingdoc/render.mjs "$REPORT_DIR/tests/cucumber" "$REPORT_DIR/tests/livingdoc"; } \
      || { echo "livingdoc: render failed"; return 0; }
    [ -f "$REPORT_DIR/tests/index.html" ] || cat > "$REPORT_DIR/tests/index.html" <<'HTML'
<!doctype html><html><head><meta charset="utf-8"><title>tests</title>
<meta http-equiv="refresh" content="0; url=livingdoc/"></head>
<body><a href="livingdoc/">living documentation</a></body></html>
HTML
  else
    echo "livingdoc: npm not found - skipping living-doc report"
  fi
}

# kind_badge <name> <label> <message> <color> - badges/<name>.json, a shields.io endpoint
# badge. <name> is one the script declares in its "# badges:" line; the kind also writes a
# report/<name>/.
kind_badge() {
  mkdir -p "$BADGE_DIR"
  jq -nc --arg label "$2" --arg message "$3" --arg color "$4" \
    '{schemaVersion: 1, label: $label, message: $message, color: $color}' > "$BADGE_DIR/$1.json"
}

# kind_badge_count <name> <label> <passed> <total> - "<passed>/<total> passed"; green only
# when every one passed.
kind_badge_count() {
  local color=red
  [ "$3" = "$4" ] && color=brightgreen
  kind_badge "$1" "$2" "$3/$4 passed" "$color"
}

# kind_badge_percent <name> <label> <percent> - "<percent>%"; >=80 brightgreen,
# >=60 yellowgreen, below that red.
kind_badge_percent() {
  kind_badge "$1" "$2" "$3%" "$(awk -v s="$3" 'BEGIN {
    if (s >= 80) print "brightgreen"; else if (s >= 60) print "yellowgreen"; else print "red" }')"
}

# kind_scenarios_report - result/scenarios.json -> report/scenarios/index.html: a type x
# status table, then one table of every entry - feature, scenario, examples, type, tags,
# status, location, note. "attention" marks untyped, missing, failed, and todo
# happy/negative/error entries without a note.
kind_scenarios_report() {
  [ -f "$RESULT_DIR/scenarios.json" ] || return 0
  mkdir -p "$REPORT_DIR/scenarios"
  jq -r '
    ["happy","boundary","negative","error","concurrency","security","regression","untyped"] as $types
    | ["passed","failed","todo","missing"] as $statuses
    | .scenarios as $all
    | def attention: .type == "untyped" or .status == "missing" or .status == "failed"
        or (.status == "todo" and .note == "" and (.type == "happy" or .type == "negative" or .type == "error"));
    "<!doctype html><html><head><meta charset=\"utf-8\"><title>Scenarios</title>",
    "<style>td,th{border:1px solid #999;padding:2px 6px;text-align:left}table{border-collapse:collapse}.attention{background:#fdd}</style></head><body>",
    "<h1>Scenarios</h1><h2>By type</h2><table><tr><th>type</th>" + ($statuses | map("<th>\(.)</th>") | join("")) + "</tr>",
    ($types[] as $t | "<tr><td>\($t)</td>" + ($statuses | map(. as $s | "<td>\([$all[] | select(.type == $t and .status == $s)] | length)</td>") | join("")) + "</tr>"),
    "</table>",
    "<h2>Every scenario</h2><table><tr><th>feature</th><th>scenario</th><th>examples</th><th>type</th><th>tags</th><th>status</th><th>location</th><th>note</th></tr>",
    ($all | sort_by(.feature, .uri, .line)[] |
      "<tr\(if attention then " class=\"attention\"" else "" end)><td>\(.feature | @html)</td><td>\(.scenario | @html)</td><td>\(.examples | @html)</td><td>\(.type)</td><td>\((.tags // []) | join(" ") | @html)</td><td>\(.status)</td><td>\(.uri | @html):\(.line)</td><td>\(.note | @html)</td></tr>"),
    "</table></body></html>"
  ' "$RESULT_DIR/scenarios.json" > "$REPORT_DIR/scenarios/index.html"
}
