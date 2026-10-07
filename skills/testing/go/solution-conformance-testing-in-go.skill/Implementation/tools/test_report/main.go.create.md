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
```go
// Command test_report builds the report directory from what the test kinds
// left in the work directory, per the solution-conformance-testing report
// contract: badges and the scenario page from the normalized result/*.json
// files, native reports copied as they are. It never parses a tool's native
// report format.
package main

import (
	"encoding/json"
	"fmt"
	"html"
	"io"
	"os"
	"path/filepath"
	"strings"
)

type unitResult struct {
	Total  int `json:"total"`
	Passed int `json:"passed"`
	Failed int `json:"failed"`
}

type coverageResult struct {
	LinePct float64 `json:"linePct"`
}

type mutationResult struct {
	Score float64 `json:"score"`
}

type badge struct {
	SchemaVersion int    `json:"schemaVersion"`
	Label         string `json:"label"`
	Message       string `json:"message"`
	Color         string `json:"color"`
}

func main() {
	if err := run(); err != nil {
		fmt.Fprintln(os.Stderr, "test_report:", err)
		os.Exit(1)
	}
}

func run() error {
	if err := os.MkdirAll(filepath.Join(reportDir(), "reports"), 0o755); err != nil {
		return err
	}
	if err := os.MkdirAll(filepath.Join(reportDir(), "badges"), 0o755); err != nil {
		return err
	}

	// Every kinds/<kind>/report/<name>/ becomes reports/<name>/, so a new
	// kind's report is published without changing this tool.
	sources, err := filepath.Glob(filepath.Join(workDir(), "kinds", "*", "report", "*"))
	if err != nil {
		return err
	}
	for _, src := range sources {
		info, statErr := os.Stat(src)
		if statErr != nil || !info.IsDir() {
			continue
		}
		dst := filepath.Join(reportDir(), "reports", filepath.Base(src))
		if _, statErr := os.Stat(dst); statErr == nil {
			return fmt.Errorf("two test kinds wrote a report named %q", filepath.Base(src))
		}
		if err := copyDir(src, dst); err != nil {
			return err
		}
	}

	if err := writeUnitBadge(); err != nil {
		return err
	}
	if err := writeCoverageBadge(); err != nil {
		return err
	}
	if err := writeMutationBadge(); err != nil {
		return err
	}
	if err := writeScenariosPage(); err != nil {
		return err
	}

	page, err := os.ReadFile(filepath.Join("report-template", "index.html"))
	if err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(reportDir(), "index.html"), page, 0o644)
}

// workDir and reportDir are chosen by the caller of `make test-report`;
// tools/testing/testing.mk exports them.
func workDir() string {
	if d := os.Getenv("TEST_WORK_DIR"); d != "" {
		return d
	}
	return "tmp/testing"
}

func reportDir() string {
	if d := os.Getenv("TEST_REPORT_DIR"); d != "" {
		return d
	}
	return filepath.Join(workDir(), "report")
}

// readResult returns the normalized result file of that name from whichever
// test kind wrote it - result names are unique across kinds.
func readResult(name string) ([]byte, error) {
	matches, err := filepath.Glob(filepath.Join(workDir(), "kinds", "*", "result", name))
	if err != nil {
		return nil, err
	}
	if len(matches) == 0 {
		return nil, os.ErrNotExist
	}
	return os.ReadFile(matches[0])
}

func writeUnitBadge() error {
	data, err := readResult("unit-test.json")
	if err != nil {
		return nil // the unit kind left no result
	}
	var r unitResult
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}
	color := "brightgreen"
	if r.Failed > 0 {
		color = "red"
	}
	return writeBadge("tests", "tests", fmt.Sprintf("%d/%d", r.Passed, r.Total), color)
}

func writeCoverageBadge() error {
	data, err := readResult("coverage-test.json")
	if err != nil {
		return nil // coverage is reported only in a report run
	}
	var r coverageResult
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}
	return writeBadge("coverage", "coverage", fmt.Sprintf("%.1f%%", r.LinePct), pctColor(r.LinePct))
}

func writeMutationBadge() error {
	data, err := readResult("mutation-test.json")
	if err != nil {
		return nil // the mutation kind did not run or skipped itself
	}
	var r mutationResult
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}
	return writeBadge("mutation", "mutation score", fmt.Sprintf("%.1f%%", r.Score), pctColor(r.Score))
}

// pctColor follows the parent contract: >=80 brightgreen / >=60 yellowgreen / else red.
func pctColor(pct float64) string {
	switch {
	case pct >= 80:
		return "brightgreen"
	case pct >= 60:
		return "yellowgreen"
	default:
		return "red"
	}
}

// writeBadge writes badges/{file}.json - the name its report has under
// reports/, independent of the human-readable label.
func writeBadge(file, label, message, color string) error {
	data, err := json.Marshal(badge{SchemaVersion: 1, Label: label, Message: message, Color: color})
	if err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(reportDir(), "badges", file+".json"), data, 0o644)
}

func copyDir(src, dst string) error {
	return filepath.WalkDir(src, func(path string, d os.DirEntry, err error) error {
		if err != nil {
			return err
		}
		rel, err := filepath.Rel(src, path)
		if err != nil {
			return err
		}
		target := filepath.Join(dst, rel)
		if d.IsDir() {
			return os.MkdirAll(target, 0o755)
		}
		in, err := os.Open(path)
		if err != nil {
			return err
		}
		defer in.Close()
		out, err := os.Create(target)
		if err != nil {
			return err
		}
		defer out.Close()
		_, err = io.Copy(out, in)
		return err
	})
}

type scenarioEntry struct {
	Feature  string `json:"feature"`
	Scenario string `json:"scenario"`
	Examples string `json:"examples"`
	URI      string `json:"uri"`
	Line     int    `json:"line"`
	Type     string `json:"type"`
	Status   string `json:"status"`
	Note     string `json:"note"`
}

var (
	scenarioTypes    = []string{"happy", "boundary", "negative", "error", "concurrency", "security", "regression", "untyped"}
	scenarioStatuses = []string{"passed", "failed", "todo", "missing"}
)

// writeScenariosPage renders reports/scenarios/index.html from
// result/scenarios.json: a type x status table, then every entry grouped
// by feature. Rows needing attention are marked "attention".
func writeScenariosPage() error {
	data, err := readResult("scenarios.json")
	if err != nil {
		return nil // the unit kind left no result
	}
	var r struct {
		Scenarios []scenarioEntry `json:"scenarios"`
	}
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}

	counts := map[string]map[string]int{}
	for _, s := range r.Scenarios {
		if counts[s.Type] == nil {
			counts[s.Type] = map[string]int{}
		}
		counts[s.Type][s.Status]++
	}

	var b strings.Builder
	b.WriteString(`<!doctype html><html><head><meta charset="utf-8"><title>Scenarios</title>
<style>td,th{border:1px solid #999;padding:2px 6px}table{border-collapse:collapse}.attention{background:#fdd}</style></head><body>
<h1>Scenarios</h1><h2>By type</h2><table><tr><th>type</th>`)
	for _, st := range scenarioStatuses {
		fmt.Fprintf(&b, "<th>%s</th>", st)
	}
	b.WriteString("</tr>")
	for _, ty := range scenarioTypes {
		fmt.Fprintf(&b, "<tr><td>%s</td>", ty)
		for _, st := range scenarioStatuses {
			fmt.Fprintf(&b, "<td>%d</td>", counts[ty][st])
		}
		b.WriteString("</tr>")
	}
	b.WriteString("</table>")

	feature := "\x00"
	for _, s := range r.Scenarios {
		if s.Feature != feature {
			if feature != "\x00" {
				b.WriteString("</table>")
			}
			feature = s.Feature
			fmt.Fprintf(&b, "<h2>%s</h2><table><tr><th>scenario</th><th>examples</th><th>type</th><th>status</th><th>location</th><th>note</th></tr>", html.EscapeString(feature))
		}
		class := ""
		if needsAttention(s) {
			class = ` class="attention"`
		}
		fmt.Fprintf(&b, "<tr%s><td>%s</td><td>%s</td><td>%s</td><td>%s</td><td>%s:%d</td><td>%s</td></tr>",
			class, html.EscapeString(s.Scenario), html.EscapeString(s.Examples), s.Type, s.Status,
			html.EscapeString(s.URI), s.Line, html.EscapeString(s.Note))
	}
	if feature != "\x00" {
		b.WriteString("</table>")
	}
	b.WriteString("</body></html>")

	if err := os.MkdirAll(filepath.Join(reportDir(), "reports", "scenarios"), 0o755); err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(reportDir(), "reports", "scenarios", "index.html"), []byte(b.String()), 0o644)
}

// needsAttention follows the parent contract: untyped, missing, failed, and
// todo happy/negative/error entries without a note.
func needsAttention(s scenarioEntry) bool {
	switch {
	case s.Type == "untyped", s.Status == "missing", s.Status == "failed":
		return true
	case s.Status == "todo" && s.Note == "" && (s.Type == "happy" || s.Type == "negative" || s.Type == "error"):
		return true
	}
	return false
}
```

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
