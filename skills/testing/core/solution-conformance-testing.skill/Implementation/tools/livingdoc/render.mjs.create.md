---
description: One stack-independent script rendering a standard Cucumber report into the living-doc HTML
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing
  - element/tools-livingdoc-render-mjs
---

# Goals
- Turn whatever standard Cucumber report the runner wrote into `$TEST_KIND_DIR/report/tests/livingdoc/`, choosing the renderer by protocol, with no stack-specific code.

# Core Principles
- Input is `$TEST_KIND_DIR/report/tests/cucumber/`: `*.ndjson` files are Cucumber Messages, `*.json` files are classic Cucumber JSON. A runner writes one protocol only.
- Cucumber Messages → `@cucumber/html-formatter`, one page per `.ndjson` file (plus an index page when there are several). Classic JSON → `multiple-cucumber-html-reporter`, all files into one report.
- Classic JSON is normalized in a temporary copy before rendering: a feature with no `tags` gets `tags: []` (godog omits the key). The runner's own files are never modified.
- A classic-JSON runner reports only what it ran. The optional fourth argument, the unit kind's `result/scenarios.json`, adds every entry that did not run — `todo` as a pending scenario, `broken` as a skipped one, `not-run` as an undefined one — each with its tags and one step that states the reason. A Cucumber Messages stream holds those scenarios already.
- The optional third argument is the legend (`kind_status_legend_json`) — one group per tag namespace (`@type/…`, `@category/…`, `@status/…`) and one for the results of a run, each a list of every value with its meaning, the meanings aligned in one column: shown in the footer of the classic-JSON report and on the index of several Messages reports. A single Messages report is the formatter's own page and takes none.

# Implementation changes
`tools/livingdoc/render.mjs`:
Copy verbatim to `tools/livingdoc/render.mjs`: [`assets/tools/livingdoc/render.mjs`](../../../assets/tools/livingdoc/render.mjs)

# Rule changes

## MUST
- Copy this script verbatim into every stack; never add a stack-specific branch.
  - Risk: a per-stack renderer reintroduces the per-stack report formats this step exists to remove.
  - Fix: make the runner write the standard protocol instead.
- Never modify the runner's files in `$TEST_KIND_DIR/report/tests/cucumber/`.
  - Risk: a downstream consumer reading the raw report sees altered data.
  - Fix: normalize in a temporary copy, as above.

# Check list
- [ ] `tools/livingdoc/render.mjs` is a byte-for-byte copy of the asset.
- [ ] After `make test-kind-unit` with npm available, `$TEST_KIND_DIR/report/tests/livingdoc/index.html` exists.
- [ ] For a classic-JSON stack the living doc lists a `@status/todo` and a `@status/broken` scenario with their reasons, shows every tag of every scenario, and ends with the status legend; no `[object Object]` appears on the page.
