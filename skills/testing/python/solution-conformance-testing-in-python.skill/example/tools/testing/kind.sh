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
    kind_status_legend_json > "$TEST_KIND_DIR/status-legend.json"
    { npm ci --prefix tools/livingdoc --silent \
        && node tools/livingdoc/render.mjs "$REPORT_DIR/tests/cucumber" "$REPORT_DIR/tests/livingdoc" \
             "$TEST_KIND_DIR/status-legend.json" "$RESULT_DIR/scenarios.json"; } \
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

# kind_status_legend_json - what every tag and every run status means, group by group: the
# one text behind the legend of the living doc. "shownAs" is the word the classic-JSON
# living doc uses for that status.
kind_status_legend_json() {
  cat <<'JSON'
{ "groups": [
  { "title": "@type/… - what the feature specifies",
    "where": "On the Feature line, exactly one. A feature without it fails the unit test kind.",
    "items": [
      { "name": "@type/domain", "meaning": "a business rule of an entity, a value object, a validator or a pure function - no I/O" },
      { "name": "@type/service", "meaning": "a use case: one operation end to end, its collaborators replaced at their ports" },
      { "name": "@type/api", "meaning": "an inbound adapter: how a request, a message or a command line becomes a call" },
      { "name": "@type/infrastructure", "meaning": "an outbound adapter against the real dependency or its emulator: a store, a cache, a client" },
      { "name": "@type/mapping", "meaning": "the translation between two representations of the same data, no I/O" },
      { "name": "@type/contract", "meaning": "the shape other code depends on: commands, events, a published schema" },
      { "name": "@type/tech-check", "meaning": "a technical check of the plumbing, not of a business capability: logging, configuration, packaging" } ] },
  { "title": "@category/… - what kind of test the scenario is",
    "where": "On a Scenario or an Examples block, exactly one. A scenario without it fails the unit test kind.",
    "items": [
      { "name": "@category/happy", "meaning": "valid input, the expected result" },
      { "name": "@category/boundary", "meaning": "an edge of what is allowed: empty, zero, the largest, the first, the last" },
      { "name": "@category/negative", "meaning": "invalid input is rejected with its error" },
      { "name": "@category/error", "meaning": "a dependency or the environment fails, and the program says so" },
      { "name": "@category/concurrency", "meaning": "several actors at the same time" },
      { "name": "@category/security", "meaning": "access, secrets, hostile input" },
      { "name": "@category/regression", "meaning": "a defect found once, kept fixed" } ] },
  { "title": "@status/… - the state of the scenario",
    "where": "Optional, beside the category tag.",
    "items": [
      { "name": "@status/todo", "meaning": "planned, not implemented yet - excluded from the run; the reason is the \"# todo:\" comment above the tags" },
      { "name": "@status/broken", "meaning": "implemented, known to fail - excluded from the run; the reason is the \"# broken:\" comment above the tags" },
      { "name": "@status/validated", "meaning": "a person has checked the scenario - set by a person only, removed when the scenario or its steps change" } ] },
  { "title": "Result of a run",
    "where": "What happened to the scenario in this run.",
    "items": [
      { "name": "passed", "meaning": "ran, every step held" },
      { "name": "failed", "meaning": "ran, a step did not hold" },
      { "name": "todo", "shownAs": "pending", "meaning": "excluded from the run by @status/todo" },
      { "name": "broken", "shownAs": "skipped", "meaning": "excluded from the run by @status/broken" },
      { "name": "not-run", "shownAs": "undefined", "meaning": "in a .feature file and not excluded, yet no runner executed it - a wiring defect, never a pass" } ] } ] }
JSON
}

# kind_status_legend - that legend as an HTML fragment.
kind_status_legend() {
  kind_status_legend_json | jq -r '
    "<h2>Tags and statuses</h2>",
    (.groups[] | "<h3>\(.title | @html)</h3><p>\(.where | @html)</p><table>"
      + (.items | map("<tr><td>\(.name | @html)</td><td>\(.meaning | @html)</td></tr>") | join("")) + "</table>")'
}

# kind_scenarios_report - result/scenarios.json -> report/scenarios/index.html: the status
# legend, a category x status and a type x status table, then one table of every entry.
# "attention" marks an entry without a type or a category, a failed or not-run one, and a
# todo or broken one without a reason.
kind_scenarios_report() {
  [ -f "$RESULT_DIR/scenarios.json" ] || return 0
  mkdir -p "$REPORT_DIR/scenarios"
  {
    echo '<!doctype html><html><head><meta charset="utf-8"><title>Scenarios</title>'
    echo '<style>td,th{border:1px solid #999;padding:2px 6px;text-align:left}table{border-collapse:collapse;margin-bottom:1em}.attention{background:#fdd}</style></head><body>'
    echo '<h1>Scenarios</h1>'
    jq -r '
      ["happy","boundary","negative","error","concurrency","security","regression","none"] as $categories
      | ["domain","service","api","infrastructure","mapping","contract","tech-check","none"] as $types
      | ["passed","failed","todo","broken","not-run"] as $statuses
      | .scenarios as $all
      | def attention: .type == "none" or .category == "none" or .status == "failed" or .status == "not-run"
          or ((.status == "todo" or .status == "broken") and .note == "");
        def summary($title; $key; $values):
          "<h2>\($title)</h2><table><tr><th>\($key)</th>" + ($statuses | map("<th>\(.)</th>") | join("")) + "</tr>",
          ($values[] as $v | "<tr><td>\($v)</td>" + ($statuses | map(. as $s | "<td>\([$all[] | select(.[$key] == $v and .status == $s)] | length)</td>") | join("")) + "</tr>"),
          "</table>";
      summary("By category - the kind of test"; "category"; $categories),
      summary("By type - the part of the program"; "type"; $types),
      "<h2>Every scenario</h2><table><tr><th>feature</th><th>type</th><th>scenario</th><th>examples</th><th>category</th><th>status</th><th>validated</th><th>tags</th><th>location</th><th>note</th></tr>",
      ($all | sort_by(.feature, .uri, .line)[] |
        "<tr\(if attention then " class=\"attention\"" else "" end)><td>\(.feature | @html)</td><td>\(.type)</td><td>\(.scenario | @html)</td><td>\(.examples | @html)</td><td>\(.category)</td><td>\(.status)</td><td>\(if .validated then "yes" else "" end)</td><td>\(.tags | join(" ") | @html)</td><td>\(.uri | @html):\(.line)</td><td>\(.note | @html)</td></tr>"),
      "</table>"
    ' "$RESULT_DIR/scenarios.json"
    kind_status_legend
    echo '</body></html>'
  } > "$REPORT_DIR/scenarios/index.html"
}

# kind_scenarios_check - fails when result/scenarios.json holds a feature without exactly one
# @type/... tag or a scenario without exactly one @category/... tag, and names each one. A
# kind calls it after its results are written and makes its own exit code non-zero when it
# fails.
kind_scenarios_check() {
  [ -f "$RESULT_DIR/scenarios.json" ] || return 0
  local found
  found=$(jq -r '
    (.scenarios | map(select(.type == "none")) | group_by(.uri)[]
      | "  \(.[0].uri)  Feature \"\(.[0].feature)\" - needs exactly one of @type/domain @type/service @type/api @type/infrastructure @type/mapping @type/contract @type/tech-check on its Feature line"),
    (.scenarios[] | select(.category == "none")
      | "  \(.uri):\(.line)  \(.scenario)\(if .examples != "" then " / " + .examples else "" end) - needs exactly one of @category/happy @category/boundary @category/negative @category/error @category/concurrency @category/security @category/regression")
    ' "$RESULT_DIR/scenarios.json")
  [ -z "$found" ] && return 0
  {
    echo "test-kind-$TEST_KIND: not tagged as cucumber-testing requires (\"One type tag per feature\", \"One category tag per scenario\"):"
    echo "$found"
  } >&2
  return 1
}
