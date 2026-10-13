#!/usr/bin/env bash
# Gathers what the test kinds left in the work directory into the report directory: every
# kinds/<kind>/report/<name>/ and kinds/<kind>/badges/<name>.json as it is, and the landing
# page with one line per report - its badge and its link. It computes nothing: what a kind
# reports, and how, is that kind's script. Called by `make test-report`, which exports:
#   TEST_WORK_DIR     kinds/<kind>/{report,badges}/ live below it
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

# Every kinds/<kind>/badges/<name>.json becomes badges/<name>.json: a kind draws its own.
for src in "$WORK_DIR"/kinds/*/badges/*.json; do
  [ -f "$src" ] || continue
  name=$(basename "$src")
  if [ -e "$REPORT_DIR/badges/$name" ]; then
    echo "test-report: two test kinds wrote a badge named '${name%.json}'" >&2
    exit 1
  fi
  cp "$src" "$REPORT_DIR/badges/$name"
done

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

# The landing page: report-template/index.html with its "<!-- test-reports -->" line replaced
# by one item per report of this run - "badge - link", reports with a badge first. A
# template without that line is published as it is.
LIST="$(mktemp)"
trap 'rm -f "$LIST"' EXIT
for with_badge in 1 0; do
  for dir in "$REPORT_DIR"/reports/*/; do
    [ -d "$dir" ] || continue
    name=$(basename "$dir"); badge="$REPORT_DIR/badges/$name.json"
    if [ -f "$badge" ] && [ "$with_badge" = 1 ]; then
      jq -r --arg name "$name" '"    <li><span class=\"badge\"><span class=\"badge-label\">\(.label | @html)</span><span class=\"badge-message badge-\(.color)\">\(.message | @html)</span></span> - <a href=\"reports/\($name)/\">\($name)</a></li>"' "$badge"
    elif [ ! -f "$badge" ] && [ "$with_badge" = 0 ]; then
      hint=""
      printf '    <li><a href="reports/%s/">%s</a>%s</li>\n' "$name" "$name" "$hint"
    fi
  done
done > "$LIST"
awk -v list="$LIST" '
  /<!-- test-reports -->/ { while ((getline line < list) > 0) print line; next }
  { print }' report-template/index.html > "$REPORT_DIR/index.html"
