#!/usr/bin/env bash
# Writes $TEST_KIND_DIR/result/scenarios.json (solution-conformance-testing's Scenario report).
#
# The inventory comes from every .feature file in the repository, so @status/todo and
# @status/broken entries the runner never executes are listed too. Per entry it reads:
#   @type/<x>       on the Feature line - what part of the program the feature specifies
#   @category/<x>   on the scenario or inherited - what kind of test it is
#   @status/todo, @status/broken - excluded from the run; the "# todo:" / "# broken:"
#                   comment directly above the tags is the reason
#   @status/validated - a person has checked the scenario
# The result of the run comes from $1: a JSON array of
# {"uri": "<repo-relative .feature path>", "line": <scenario or Examples row line>,
#  "status": "passed" | "failed" | "<anything else>"} produced from the runner's output.
#
# Entry = one Scenario/Example, or one Examples: block of a Scenario Outline.
# English Gherkin keywords only.
set -euo pipefail

RESULTS="${1:?usage: normalize-scenarios.sh <results.json>}"
RESULT_DIR="${TEST_KIND_DIR:?run this through a kind script}/result"
mkdir -p "$RESULT_DIR"

INVENTORY="$(mktemp)"
trap 'rm -f "$INVENTORY"' EXIT

# Nothing below the work directory is a project file: a mutation tool's sandbox there holds
# copies of the .feature files.
WORK_REL="$(realpath -m --relative-to=. "${TEST_WORK_DIR:-tmp/testing}")/"

# One TSV line per entry: feature, scenario, examples, uri, line, tags, state (todo, broken
# or empty), note, lines, and the tags of the Feature line alone.
find . \( -name .git -o -name node_modules -o -name bin -o -name obj -o -name tmp -o -name public -o -name .venv \) -prune \
  -o -name '*.feature' -print | sed 's#^\./##' | awk -v work="$WORK_REL" 'index($0, work) != 1' | sort | while IFS= read -r uri; do
  awk -v uri="$uri" '
    function trim(s) { sub(/^[ \t]+/, "", s); sub(/[ \t\r]+$/, "", s); return s }
    function after_colon(s) { sub(/^[^:]*:[ \t]*/, "", s); return s }
    function take_level(   i, n, t) {          # consume pending tags into one level
      lvl_tags = pend_tags; lvl_todo = ""
      n = split(pend_tags, t, " ")
      for (i = 1; i <= n; i++) {
        if (t[i] == "status/todo") lvl_todo = "todo"
        else if (t[i] == "status/broken" && lvl_todo == "") lvl_todo = "broken"
      }
      lvl_note = (note_line == pend_top - 1 || (pend_top == 0 && note_line == NR - 1)) ? note_text : ""
      pend_tags = ""; pend_top = 0
    }
    function emit(ex_name, ex_line, ex_tags, ex_todo, ex_note, rows,   tags, todo, note) {
      tags = feat_tags " " rule_tags " " sc_tags " " ex_tags
      todo = ""; note = ""
      if (feat_todo != "") { todo = feat_todo; note = feat_note }
      else if (rule_todo != "") { todo = rule_todo; note = rule_note }
      else if (sc_todo != "") { todo = sc_todo; note = sc_note }
      else if (ex_todo != "") { todo = ex_todo; note = ex_note }
      printf "%s\t%s\t%s\t%s\t%d\t%s\t%s\t%s\t%s\t%s\n", feat_name, sc_name, ex_name, uri, ex_line, tags, todo, note, rows, feat_tags
    }
    function flush_examples() {
      if (ex_open) emit(ex_name, ex_line, ex_tags, ex_todo, ex_note, ex_rows)
      ex_open = 0
    }
    function flush_scenario() {
      flush_examples()
      if (sc_open && !sc_outline) emit("", sc_line, "", "", "", sc_line)
      if (sc_open && sc_outline && !sc_blocks) emit("", sc_line, "", "", "", "")
      sc_open = 0
    }
    {
      line = trim($0)
      if (doc != "") { if (index(line, doc) == 1) doc = ""; next }
      if (line ~ /^("""|```)/) { doc = substr(line, 1, 3); next }
      if (line == "") next
      if (line ~ /^#/) {
        if (match(line, /^#[ \t]*(todo|broken):[ \t]*/)) { note_line = NR; note_text = substr(line, RLENGTH + 1) }
        next
      }
      if (line ~ /^@/) {
        sub(/[ \t]+#.*$/, "", line)
        n = split(line, t, /[ \t]+/)
        for (i = 1; i <= n; i++) if (t[i] ~ /^@/) pend_tags = pend_tags " " substr(t[i], 2)
        if (pend_top == 0) pend_top = NR
        next
      }
      if (line ~ /^Feature:/) {
        take_level(); feat_tags = lvl_tags; feat_todo = lvl_todo; feat_note = lvl_note
        feat_name = after_colon(line); next
      }
      if (line ~ /^Rule:/) {
        flush_scenario(); take_level(); rule_tags = lvl_tags; rule_todo = lvl_todo; rule_note = lvl_note
        next
      }
      if (line ~ /^Background:/) { flush_scenario(); pend_tags = ""; pend_top = 0; next }
      if (line ~ /^(Scenario Outline|Scenario Template|Scenario|Example):/) {
        flush_scenario(); take_level()
        sc_tags = lvl_tags; sc_todo = lvl_todo; sc_note = lvl_note
        sc_open = 1; sc_outline = (line ~ /^Scenario (Outline|Template):/); sc_blocks = 0
        sc_name = after_colon(line); sc_line = NR; next
      }
      if (line ~ /^(Examples|Scenarios):/) {
        flush_examples(); take_level()
        ex_tags = lvl_tags; ex_todo = lvl_todo; ex_note = lvl_note
        ex_open = 1; ex_name = after_colon(line); ex_line = NR; ex_rows = ""; ex_header = 0; sc_blocks++
        next
      }
      if (line ~ /^\|/ && ex_open) {
        if (!ex_header) { ex_header = 1; next }
        ex_rows = ex_rows (ex_rows == "" ? "" : ",") NR
      }
    }
    END { flush_scenario() }
  ' "$uri"
done > "$INVENTORY"

jq -R -s --slurpfile results "$RESULTS" '
  # The one allowed value of a tag namespace among the given tags ("type/service" ->
  # "service"), or "none" when there is none or more than one.
  def one($ns; $allowed): [.[] | select(startswith($ns + "/")) | ltrimstr($ns + "/")
      | select(. as $v | $allowed | index($v))] | unique
    | if length == 1 then .[0] else "none" end;
  ["domain","service","api","infrastructure","mapping","contract","tech-check"] as $types
  | ["happy","boundary","negative","error","concurrency","security","regression"] as $categories
  |
  ($results[0] | map({key: "\(.uri):\(.line)", value: .status}) | group_by(.key)
     | map({key: .[0].key, value: map(.value)}) | from_entries) as $status
  | split("\n") | map(select(length > 0) | split("\t")) | map(
      . as [$feature, $scenario, $examples, $uri, $line, $tags, $state, $note, $lines, $featureTags]
      | ($tags | split(" ") | map(select(length > 0)) | unique) as $tagList
      | ($lines | split(",") | map(select(length > 0)) | map($status["\($uri):\(.)"] // [])) as $perRow
      | {
          feature: $feature,
          type: (($featureTags // "") | split(" ") | map(select(length > 0)) | one("type"; $types)),
          scenario: $scenario, examples: $examples,
          category: ($tagList | one("category"; $categories)),
          status: (
            if $state != "" then $state
            elif any($perRow[]; index("failed")) then "failed"
            elif ($perRow | length) == 0 or any($perRow[]; index("passed") | not) then "not-run"
            else "passed" end),
          validated: ($tagList | index("status/validated") != null),
          tags: ($tagList | map("@" + .)),
          uri: $uri, line: ($line | tonumber),
          note: $note
        })
  | {scenarios: .}
' "$INVENTORY" > "$RESULT_DIR/scenarios.json"
