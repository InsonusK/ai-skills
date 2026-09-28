// Command test_report assembles public/ from the normalized tmp/result/*.json
// files, per the solution-conformance-testing report contract. It never
// parses a tool's native report format.
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
	if err := writeScenariosPage(); err != nil {
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

// writeScenariosPage renders public/scenarios/index.html from
// tmp/result/scenarios.json: a type x status table, then every entry grouped
// by feature. Rows needing attention are marked "attention".
func writeScenariosPage() error {
	data, err := os.ReadFile(filepath.Join("tmp", "result", "scenarios.json"))
	if err != nil {
		return nil // unit-test has not run yet
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

	if err := os.MkdirAll(filepath.Join("public", "scenarios"), 0o755); err != nil {
		return err
	}
	return os.WriteFile(filepath.Join("public", "scenarios", "index.html"), []byte(b.String()), 0o644)
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
