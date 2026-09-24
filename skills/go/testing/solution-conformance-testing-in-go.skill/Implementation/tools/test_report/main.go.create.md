---
description: Assembles the stack-independent public/ site from tmp/result/*.json
project_name: tools/test_report
name: test_report
element_kind: functions
change_kind: create
tags:
  - solution/conformance-testing-in-go
  - element/tools-test-report-main-go
---

# Goals
- Assemble `public/` — badges, per-kind report copies, and the landing page — reading only the normalized `tmp/result/*.json` files, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#public-site-output|the parent solution's Public site output contract]].

# Core Principles
- Reads `tmp/result/*.json` only — never re-parses `go test`'s or `gremlins`' native output.
- `coverage-test.json` and `mutation-test.json` are optional inputs (a run without `WITH_CODE_COVERAGE=true`, or before `mutation-test` has ever run, has neither) — their badges are simply omitted, never a fatal error.

# Implementation changes
```go
// Command test_report assembles public/ from the normalized tmp/result/*.json
// files, per the parent solution-conformance-testing report contract. It
// never parses a tool's native report format.
package main

import (
	"encoding/json"
	"fmt"
	"io"
	"os"
	"path/filepath"
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
	if err := os.MkdirAll("public", 0o755); err != nil {
		return err
	}

	for _, kind := range []string{"tests", "coverage", "mutation"} {
		src := filepath.Join("tmp", "report", kind)
		if _, err := os.Stat(src); err != nil {
			continue // this kind was never run in this invocation
		}
		if err := copyDir(src, filepath.Join("public", kind)); err != nil {
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

	page, err := os.ReadFile(filepath.Join("report-template", "index.html"))
	if err != nil {
		return err
	}
	return os.WriteFile(filepath.Join("public", "index.html"), page, 0o644)
}

func writeUnitBadge() error {
	data, err := os.ReadFile(filepath.Join("tmp", "result", "unit-test.json"))
	if err != nil {
		return nil // no unit-test run yet in this invocation
	}
	var r unitResult
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}
	color := "brightgreen"
	if r.Failed > 0 {
		color = "red"
	}
	return writeBadge("tests", fmt.Sprintf("%d/%d", r.Passed, r.Total), color)
}

func writeCoverageBadge() error {
	data, err := os.ReadFile(filepath.Join("tmp", "result", "coverage-test.json"))
	if err != nil {
		return nil // WITH_CODE_COVERAGE was not set
	}
	var r coverageResult
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}
	return writeBadge("coverage", fmt.Sprintf("%.1f%%", r.LinePct), pctColor(r.LinePct))
}

func writeMutationBadge() error {
	data, err := os.ReadFile(filepath.Join("tmp", "result", "mutation-test.json"))
	if err != nil {
		return nil // mutation-test has not run yet
	}
	var r mutationResult
	if err := json.Unmarshal(data, &r); err != nil {
		return err
	}
	return writeBadge("mutation score", fmt.Sprintf("%.1f%%", r.Score), pctColor(r.Score))
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

func writeBadge(label, message, color string) error {
	data, err := json.Marshal(badge{SchemaVersion: 1, Label: label, Message: message, Color: color})
	if err != nil {
		return err
	}
	name := fmt.Sprintf("%s-badge.json", slug(label))
	return os.WriteFile(filepath.Join("public", name), data, 0o644)
}

func slug(label string) string {
	out := make([]byte, 0, len(label))
	for _, r := range label {
		if r == ' ' {
			out = append(out, '-')
			continue
		}
		out = append(out, byte(r))
	}
	return string(out)
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
```

# Rule changes

## MUST
- Read only `tmp/result/*.json` to decide badge content — never `tmp/report/<kind>/`'s native files.
  - Risk: parsing a tool's native report format here duplicates the normalizers' own parsing and breaks the moment the underlying tool's report shape changes.
  - Fix: every badge's value comes from the already-normalized JSON; `tmp/report/<kind>/` is copied byte-for-byte, never read for data.
- Treat a missing `tmp/result/{coverage-test,mutation-test}.json` as "skip this badge," never as a fatal error.
  - Risk: `test-report` is also called by `test-and-report` right after `mutation-test`, but a standalone `make test-report` (or a first `unit-test` run without `WITH_CODE_COVERAGE=true`) legitimately has no mutation or coverage result yet.
  - Fix: `os.ReadFile`'s error on either optional file returns `nil` from that badge function, producing no badge for that metric rather than exiting.
- Copy `report-template/index.html` byte-for-byte into `public/index.html` — never generate its content.
  - Risk: generating the landing page here duplicates ownership of a file the parent contract states this project owns, not this tool.
  - Fix: read-then-write the file unchanged.

# Check list
- [ ] `public/index.html` is byte-identical to `report-template/index.html`.
- [ ] `public/tests-badge.json` always exists after any `unit-test` run; `public/coverage-badge.json` and `public/mutation-score-badge.json` exist only when their `tmp/result/*.json` input is present.
- [ ] Badge colors follow the parent contract's thresholds exactly.

# Unittest TestCases
- [ ] WHEN `tmp/result/coverage-test.json` is absent THEN `test_report` completes successfully with no `coverage-badge.json`
- [ ] WHEN `unit-test.json` reports `failed > 0` THEN the tests badge color is `red`
- [ ] WHEN `coverage-test.json` reports `linePct: 82.3` THEN the coverage badge color is `brightgreen`
