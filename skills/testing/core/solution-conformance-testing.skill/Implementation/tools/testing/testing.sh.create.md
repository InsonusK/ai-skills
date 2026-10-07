---
description: The checks behind the testing contract — README badges against the declaration, and the finished report against the declaration
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing
  - element/tools-testing-testing-sh
---

# Goals
- Fail with a message naming the badge when the README and the declared badges disagree.
- Record in the report how each kind ran, and fail when the report and the declaration disagree.

# Core Principles
- `readme-check` compares the README with the *declared* badges and runs no tests: a `check` run skips kinds, so the badges a run produced cannot be the reference.
- A badge is recognised in the README by `badges/{name}.json` in its URL — the host and the path before it are the publisher's business.
- `report-finish` writes `run.json` (purpose, and per kind `ran` / `skipped` / `missing` with its note), requires a same-named report for every badge, and in a `report` run requires every declared badge of every kind that ran.

# Implementation changes
`tools/testing/testing.sh`:
Copy verbatim to `tools/testing/testing.sh`: [`assets/tools/testing/testing.sh`](../../../assets/tools/testing/testing.sh)

# Rule changes

## MUST
- Copy this file verbatim into every project; never edit it there.
  - Risk: a project-local change weakens a check only in that project.
  - Fix: change the declaration (`TEST_BADGES_{kind}`) or the README, never the check.

# Check list
- [ ] `tools/testing/testing.sh` is a byte-for-byte copy of the asset.
- [ ] `make test-readme-check` names a missing badge and a badge no kind declares.
