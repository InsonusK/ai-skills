---
description: One stack-independent script rendering a standard Cucumber report into the living-doc HTML
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing
  - element/tools-livingdoc-render-mjs
---

# Goals
- Turn whatever standard Cucumber report the runner wrote into `$TEST_KIND_DIR/report/tests/livingdoc/`, using one renderer for both protocols, with no stack-specific code.

# Core Principles
- Input is `$TEST_KIND_DIR/report/tests/cucumber/`: `*.ndjson` files are Cucumber Messages, `*.json` files are classic Cucumber JSON. A runner writes one protocol only.
- Cucumber Messages → classic JSON with actual step results; both protocols → `multiple-cucumber-html-reporter`, all files into one report.
- Classic JSON is normalized in a temporary copy before rendering: a feature with no `tags` gets `tags: []` (godog omits the key). The runner's own files are never modified.
- A classic-JSON runner reports only what it ran. The optional fourth argument, the unit kind's `result/scenarios.json`, adds every entry that did not run — `todo` as a pending scenario, `broken` as a skipped one, `not-run` as an undefined one — each with its tags and one step that states the reason. This also completes the Messages view; unexecuted pickles alone cannot carry run results or exclusion reasons.
- The optional third argument is the status legend (`kind_status_legend_json`): shown in the footer of every report.

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
- [ ] For every stack the living doc lists a `@status/todo` and a `@status/broken` scenario with their reasons, shows every tag of every scenario, and ends with the status legend; no `[object Object]` appears on the page.
