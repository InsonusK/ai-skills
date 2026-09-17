---
description: Normalizes gremlins' mutation-testing report into tmp/result/mutation-test.json
project_name: tools/normalize_mutation
name: normalize_mutation
element_kind: functions
change_kind: create
tags:
  - solution/go-conformance-testing
  - element/tools-normalize-mutation-main-go
---

# Goals
- Turn `gremlins`' own JSON report into the normalized `{"killed","survived","timedout","noCoverage","score"}` shape [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent solution's report contract]] defines.

# Core Principles
- Reads `gremlins`' native report as its one input argument's path; never re-runs or shells out to `gremlins` itself — the `Makefile` target owns invoking the tool, this program only normalizes its output.
- `score` is `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `"0.0"` when nothing was mutated — exactly the parent contract's formula, not a re-derivation.

# Implementation changes
```go
// Command normalize_mutation reads gremlins' own JSON mutation report (its
// path is argv[1]) and writes the normalized tmp/result/mutation-test.json
// the parent solution-conformance-testing report contract defines, plus a
// small human-readable tmp/report/mutation/index.html table.
//
// gremlins' exact field names are version-dependent; this program reads a
// flat list of per-mutant statuses (Mutants []struct{Status string}) and
// case-insensitively maps each status to one of the four buckets. Verify
// this mapping against the gremlins version actually installed
// (`gremlins unleash --output <path>` and inspect the result) before
// relying on it, and adjust the struct tags here if a newer gremlins
// renames or restructures the report.
package main

import (
	"encoding/json"
	"fmt"
	"os"
	"strings"
)

type gremlinsReport struct {
	Mutants []struct {
		Status string `json:"status"`
	} `json:"mutants"`
}

type normalized struct {
	Killed     int     `json:"killed"`
	Survived   int     `json:"survived"`
	TimedOut   int     `json:"timedout"`
	NoCoverage int     `json:"noCoverage"`
	Score      float64 `json:"score"`
}

func main() {
	if len(os.Args) != 2 {
		fmt.Fprintln(os.Stderr, "usage: normalize_mutation <gremlins-report.json>")
		os.Exit(1)
	}
	if err := run(os.Args[1]); err != nil {
		fmt.Fprintln(os.Stderr, "normalize_mutation:", err)
		os.Exit(1)
	}
}

func run(reportPath string) error {
	data, err := os.ReadFile(reportPath)
	if err != nil {
		return err
	}
	var report gremlinsReport
	if err := json.Unmarshal(data, &report); err != nil {
		return err
	}

	n := normalized{}
	for _, m := range report.Mutants {
		switch strings.ToUpper(m.Status) {
		case "KILLED":
			n.Killed++
		case "SURVIVED":
			n.Survived++
		case "TIMED OUT", "TIMEDOUT", "TIMED_OUT":
			n.TimedOut++
		case "NOT COVERED", "NOTCOVERED", "NOT_COVERED", "NO COVERAGE":
			n.NoCoverage++
		}
	}

	total := n.Killed + n.Survived + n.TimedOut + n.NoCoverage
	if total > 0 {
		n.Score = round1(float64(n.Killed) / float64(total) * 100)
	}

	if err := os.MkdirAll("tmp/result", 0o755); err != nil {
		return err
	}
	out, err := json.Marshal(n)
	if err != nil {
		return err
	}
	if err := os.WriteFile("tmp/result/mutation-test.json", out, 0o644); err != nil {
		return err
	}

	if err := os.MkdirAll("tmp/report/mutation", 0o755); err != nil {
		return err
	}
	return os.WriteFile("tmp/report/mutation/index.html", []byte(renderHTML(n)), 0o644)
}

func round1(v float64) float64 {
	return float64(int(v*10+0.5)) / 10
}

func renderHTML(n normalized) string {
	return fmt.Sprintf(`<!doctype html><html><head><meta charset="utf-8"><title>Mutation report</title></head>
<body><table border="1">
<tr><th>Killed</th><th>Survived</th><th>Timed out</th><th>No coverage</th><th>Score</th></tr>
<tr><td>%d</td><td>%d</td><td>%d</td><td>%d</td><td>%.1f%%</td></tr>
</table></body></html>`, n.Killed, n.Survived, n.TimedOut, n.NoCoverage, n.Score)
}
```

# Rule changes

## MUST
- Never shell out to `gremlins` from this program — it only reads the report path given as its argument.
  - Risk: this tool re-running the mutation tool duplicates the `Makefile` target's own invocation and doubles the run time.
  - Fix: `mutation-test` runs `gremlins` itself and passes the resulting report's path as this program's sole argument.
- `score` must use the exact formula from [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent contract]] — `killed / (killed+survived+timedout+noCoverage) * 100`, rounded to 1 decimal, `0.0` when the denominator is `0`.
  - Risk: a different rounding or a different denominator (e.g. including `NOT_VIABLE`/compile-error mutants) produces a badge value that does not match what a human reading the native `gremlins` report would compute.
  - Fix: sum exactly the four bucketed counts as the denominator; guard the zero-denominator case explicitly.

# Check list
- [ ] `tmp/result/mutation-test.json` matches `{"killed","survived","timedout","noCoverage","score"}` with `score` a 1-decimal number.
- [ ] A report with zero mutants produces `"score": 0.0`, not `NaN` or a division-by-zero panic.

# Unittest TestCases
- [ ] WHEN the report has 3 killed and 1 survived mutant THEN `score` is `75.0`
- [ ] WHEN the report has zero mutants THEN `score` is `0.0` and the program does not panic
- [ ] WHEN a mutant's status is unrecognized THEN it is not counted in any bucket, and the denominator reflects only the four recognized buckets
