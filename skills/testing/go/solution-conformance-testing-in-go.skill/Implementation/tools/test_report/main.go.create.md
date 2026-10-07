---
description: Builds the stack-independent report directory — badges, report copies, scenario page — from every test kind's result/*.json and report/ folders
project_name: tools/test_report
name: test_report
element_kind: functions
change_kind: create
tags:
  - solution/conformance-testing-in-go
  - element/tools-test-report-main-go
---

# Goals
- Assemble `$TEST_REPORT_DIR/` — badges, per-kind report copies, the scenario page, and the landing page — reading only the normalized `$TEST_KIND_DIR/result/*.json` files, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-output|the parent solution's Public site output contract]].

# Core Principles
- Reads `$TEST_KIND_DIR/result/*.json` only — never re-parses `go test`'s or `gremlins`' native output.
- `$TEST_REPORT_DIR/reports/scenarios/index.html` is rendered from `$TEST_KIND_DIR/result/scenarios.json` alone, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md#scenario-report|the parent solution's Scenario report]].
- `coverage-test.json`, `mutation-test.json`, and `scenarios.json` are optional inputs (a `check` run reports no coverage, and a kind that skipped itself leaves no result) — their badges are simply omitted, never a fatal error.

# Implementation changes
Copy verbatim to `tools/test_report/main.go`: [`assets/tools/test_report/main.go`](../../../assets/tools/test_report/main.go)

# Rule changes

## MUST
- Read only `$TEST_KIND_DIR/result/*.json` to decide badge content — never `$TEST_KIND_DIR/report/<kind>/`'s native files.
  - Risk: parsing a tool's native report format here duplicates the normalizers' own parsing and breaks the moment the underlying tool's report shape changes.
  - Fix: every badge's value comes from the already-normalized JSON; `$TEST_KIND_DIR/report/<kind>/` is copied byte-for-byte, never read for data.
- Treat a missing `$TEST_KIND_DIR/result/{coverage-test,mutation-test}.json` as "skip this badge," never as a fatal error.
  - Risk: `test-report` is also called by `test-and-report` right after `test-kind-mutation`, but a standalone `make test-report` (or a `check` run) legitimately has no mutation or coverage result.
  - Fix: `os.ReadFile`'s error on either optional file returns `nil` from that badge function, producing no badge for that metric rather than exiting.
- Mark a scenario row as needing attention exactly when the parent contract says so: `untyped`, `missing`, `failed`, or a `todo` entry of type `happy`/`negative`/`error` without a note.
  - Risk: a looser rule lets a planned-but-unexplained negative case blend in with the finished rows; a stricter one trains readers to ignore the highlight.
  - Fix: keep `needsAttention` aligned with the parent contract's list.
- Copy `report-template/index.html` byte-for-byte into `$TEST_REPORT_DIR/index.html` — never generate its content.
  - Risk: generating the landing page here duplicates ownership of a file the parent contract states this project owns, not this tool.
  - Fix: read-then-write the file unchanged.

# Check list
- [ ] `$TEST_REPORT_DIR/index.html` is byte-identical to `report-template/index.html`.
- [ ] `$TEST_REPORT_DIR/badges/tests.json` always exists after any `test-kind-unit` run; `$TEST_REPORT_DIR/badges/coverage.json` and `$TEST_REPORT_DIR/badges/mutation.json` exist only when their `$TEST_KIND_DIR/result/*.json` input is present.
- [ ] Badge colors follow the parent contract's thresholds exactly.
- [ ] `$TEST_REPORT_DIR/reports/scenarios/index.html` exists whenever `$TEST_KIND_DIR/result/scenarios.json` does, with the type × status table first.

# Unittest TestCases
- [ ] WHEN `$TEST_KIND_DIR/result/coverage-test.json` is absent THEN `test_report` completes successfully with no `badges/coverage.json`
- [ ] WHEN `unit-test.json` reports `failed > 0` THEN the tests badge color is `red`
- [ ] WHEN `coverage-test.json` reports `linePct: 82.3` THEN the coverage badge color is `brightgreen`
- [ ] WHEN `scenarios.json` has one `todo` `negative` entry with an empty note THEN its row carries `class="attention"`
- [ ] WHEN `scenarios.json` is absent THEN `test_report` completes successfully with no `$TEST_REPORT_DIR/reports/scenarios/`
