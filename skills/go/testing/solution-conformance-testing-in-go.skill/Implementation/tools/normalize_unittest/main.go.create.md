---
description: Normalizes go test -json output into tmp/result/unit-test.json, counting each leaf test once
project_name: tools/normalize_unittest
name: normalize_unittest
element_kind: functions
change_kind: create
tags:
  - solution/conformance-testing-in-go
  - element/tools-normalize-unittest-main-go
---

# Goals
- Turn `go test -json`'s event stream into the normalized `{"total","passed","failed"}` shape [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md#report-contract|the parent solution's report contract]] defines, counting a godog scenario once — not once for itself and once again for its parent `TestFeatures` umbrella test.

# Core Principles
- `go test -json` reports a pass/fail/skip event for `TestFeatures` itself *and* for every `TestFeatures/{scenario}` subtest godog drives through `t.Run` — only the leaf (deepest) name is a real, independent test result; its ancestors are just containers.

# Implementation changes
```go
// Command normalize_unittest reads `go test -json` events from stdin and
// writes the normalized tmp/result/unit-test.json the parent
// solution-conformance-testing report contract defines. It counts each leaf
// test exactly once: a godog scenario run as a Go subtest of TestFeatures
// reports its own pass/fail alongside TestFeatures' own — only the deepest
// name per branch is counted.
package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"os"
	"strings"
)

type testEvent struct {
	Action  string
	Package string
	Test    string
}

func main() {
	if err := run(); err != nil {
		fmt.Fprintln(os.Stderr, "normalize_unittest:", err)
		os.Exit(1)
	}
}

func run() error {
	results := map[string]string{} // "{package}/{test}" -> last pass/fail/skip action seen
	sc := bufio.NewScanner(os.Stdin)
	sc.Buffer(make([]byte, 64*1024), 1024*1024)
	for sc.Scan() {
		var ev testEvent
		if err := json.Unmarshal(sc.Bytes(), &ev); err != nil {
			continue // a non-JSON line (build output on stderr interleaved by a wrapper); ignore
		}
		if ev.Test == "" {
			continue // package-level event, not a test result
		}
		switch ev.Action {
		case "pass", "fail", "skip":
			results[ev.Package+"/"+ev.Test] = ev.Action
		}
	}
	if err := sc.Err(); err != nil {
		return err
	}

	total, passed, failed := 0, 0, 0
	for _, name := range leafNames(results) {
		total++
		switch results[name] {
		case "pass":
			passed++
		case "fail":
			failed++
		}
	}

	if err := os.MkdirAll("tmp/result", 0o755); err != nil {
		return err
	}
	data, err := json.Marshal(struct {
		Total  int `json:"total"`
		Passed int `json:"passed"`
		Failed int `json:"failed"`
	}{total, passed, failed})
	if err != nil {
		return err
	}
	return os.WriteFile("tmp/result/unit-test.json", data, 0o644)
}

// leafNames returns every key with no other key nested under it (no other
// key starts with "key/"), i.e. every test with no subtest of its own.
func leafNames(results map[string]string) []string {
	names := make([]string, 0, len(results))
	for k := range results {
		names = append(names, k)
	}
	leaves := make([]string, 0, len(names))
	for _, k := range names {
		prefix := k + "/"
		hasChild := false
		for _, other := range names {
			if strings.HasPrefix(other, prefix) {
				hasChild = true
				break
			}
		}
		if !hasChild {
			leaves = append(leaves, k)
		}
	}
	return leaves
}
```

# Rule changes

## MUST
- Count only leaf test names — never every `pass`/`fail` event with a non-empty `Test` field.
  - Violation: summing every `action == "pass"` event's count directly from the stream.
  - Risk: `TestFeatures` and each of its `N` godog subtests all report their own pass/fail, so a naive count reports `N+1` tests for what is really `N` scenarios, corrupting the badge and the report's total.
  - Fix: buffer every test's last-seen action, then count only names with no other name nested under them (`leafNames`).
- Never `exit` non-zero because a test failed — this tool always writes its normalized result and returns `0`; the failure is visible in `"failed"` and via `go test`'s own exit code, which `make unit-test`'s `set -o pipefail` already propagates.
  - Risk: this tool exiting non-zero on a failed test would compete with the pipeline's own exit-code propagation and risk masking which command actually failed.
  - Fix: only `os.Exit(1)` on a real tool error (a malformed stdin read, a write failure) — never because the normalized counts show a failure.

# Check list
- [ ] `tmp/result/unit-test.json` matches `{"total": <int>, "passed": <int>, "failed": <int>}` exactly.
- [ ] A `TestFeatures` run with 3 passing godog scenarios and no other test produces `{"total": 3, "passed": 3, "failed": 0}`, not `{"total": 4, ...}`.

# Unittest TestCases
- [ ] WHEN the stream has a `pass` event for `TestFeatures` and `pass` events for two `TestFeatures/{scenario}` subtests THEN `leafNames` returns only the two subtest names
- [ ] WHEN a test has no subtest THEN it is counted as its own leaf
- [ ] WHEN a subtest fails while its parent's own event (if any) reports pass THEN the leaf's `fail` action is what gets counted
