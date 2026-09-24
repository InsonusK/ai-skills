#!/usr/bin/env bash
# Writes tmp/result/scenarios.json (solution-conformance-testing's Scenario report).
#
# The inventory comes from every .feature file in the repository, so @todo entries the
# runner never executes are listed too. The status comes from $1: a JSON array of
# {"uri": "<repo-relative .feature path>", "line": <scenario or Examples row line>,
#  "status": "passed" | "failed" | "<anything else>"} produced from the runner's output.
#
# Entry = one Scenario/Example, or one Examples: block of a Scenario Outline.
# English Gherkin keywords only.
set -euo pipefail

RESULTS="${1:?usage: normalize-scenarios.sh <results.json>}"
RESULT_DIR="tmp/result"
mkdir -p "$RESULT_DIR"

INVENTORY="$(mktemp)"
trap 'rm -f "$INVENTORY"' EXIT

# One TSV line per entry: feature, scenario, examples, uri, line, tags, todo, note, lines.
find . \( -name .git -o -name node_modules -o -name bin -o -name obj -o -name tmp -o -name public -o -name .venv \) -prune \
  -o -name '*.feature' -print | sed 's#^\./##' | sort | while IFS= read -r uri; do
  awk -v uri="$uri" '
    function trim(s) { sub(/^[ \t]+/, "", s); sub(/[ \t\r]+$/, "", s); return s }
    function after_colon(s) { sub(/^[^:]*:[ \t]*/, "", s); return s }
    function take_level(   i, n, t) {          # consume pending tags into one level
      lvl_tags = pend_tags; lvl_todo = 0
      n = split(pend_tags, t, " ")
      for (i = 1; i <= n; i++) if (t[i] == "todo") lvl_todo = 1
      lvl_note = (note_line == pend_top - 1 || (pend_top == 0 && note_line == NR - 1)) ? note_text : ""
      pend_tags = ""; pend_top = 0
    }
    function emit(ex_name, ex_line, ex_tags, ex_todo, ex_note, rows,   tags, todo, note) {
      tags = feat_tags " " rule_tags " " sc_tags " " ex_tags
      todo = 0; note = ""
      if (feat_todo) { todo = 1; note = feat_note }
      else if (rule_todo) { todo = 1; note = rule_note }
      else if (sc_todo) { todo = 1; note = sc_note }
      else if (ex_todo) { todo = 1; note = ex_note }
      printf "%s\t%s\t%s\t%s\t%d\t%s\t%d\t%s\t%s\n", feat_name, sc_name, ex_name, uri, ex_line, tags, todo, note, rows
    }
    function flush_examples() {
      if (ex_open) emit(ex_name, ex_line, ex_tags, ex_todo, ex_note, ex_rows)
      ex_open = 0
    }
    function flush_scenario() {
      flush_examples()
      if (sc_open && !sc_outline) emit("", sc_line, "", 0, "", sc_line)
      if (sc_open && sc_outline && !sc_blocks) emit("", sc_line, "", 0, "", "")
      sc_open = 0
    }
    {
      line = trim($0)
      if (doc != "") { if (index(line, doc) == 1) doc = ""; next }
      if (line ~ /^("""|```)/) { doc = substr(line, 1, 3); next }
      if (line == "") next
      if (line ~ /^#/) {
        if (match(line, /^#[ \t]*todo:[ \t]*/)) { note_line = NR; note_text = substr(line, RLENGTH + 1) }
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
  def types: ["happy","boundary","negative","error","concurrency","security","regression"];
  ($results[0] | map({key: "\(.uri):\(.line)", value: .status}) | group_by(.key)
     | map({key: .[0].key, value: map(.value)}) | from_entries) as $status
  | split("\n") | map(select(length > 0) | split("\t")) | map(
      . as [$feature, $scenario, $examples, $uri, $line, $tags, $todo, $note, $lines]
      | ($tags | split(" ") | map(select(length > 0))) as $tagList
      | ([$tagList[] | select(. as $t | types | index($t))] | unique) as $typeTags
      | ($lines | split(",") | map(select(length > 0)) | map($status["\($uri):\(.)"] // [])) as $perRow
      | {
          feature: $feature, scenario: $scenario, examples: $examples,
          uri: $uri, line: ($line | tonumber),
          type: (if ($typeTags | length) == 1 then $typeTags[0] else "untyped" end),
          status: (
            if $todo == "1" then "todo"
            elif any($perRow[]; index("failed")) then "failed"
            elif ($perRow | length) == 0 or any($perRow[]; index("passed") | not) then "missing"
            else "passed" end),
          note: $note
        })
  | {scenarios: .}
' "$INVENTORY" > "$RESULT_DIR/scenarios.json"
