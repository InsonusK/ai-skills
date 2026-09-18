// Command normalize_unittest reads `go test -json` events from stdin and
// writes the normalized tmp/result/unit-test.json the solution-conformance-testing
// report contract defines. It counts each leaf test exactly once: a godog
// scenario run as a Go subtest of TestFeatures reports its own pass/fail
// alongside TestFeatures' own - only the deepest name per branch is counted.
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
