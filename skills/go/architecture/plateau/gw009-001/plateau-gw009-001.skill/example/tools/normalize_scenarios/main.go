// Command normalize_scenarios writes the normalized tmp/result/scenarios.json
// the parent solution-conformance-testing Scenario report defines. The
// inventory comes from every .feature file under the repository (so @todo
// entries godog never runs are listed too); the status of each entry comes
// from the `go test -json` event stream whose path is argv[1].
//
// godog runs every pickle as a Go subtest named TestFeatures/{pickle name};
// `go test` rewrites spaces to "_" and suffixes repeated names with "#NN".
// A pickle's status is looked up by that rewritten name across all packages.
package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"io/fs"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"unicode"

	gherkin "github.com/cucumber/gherkin/go/v42"
	messages "github.com/cucumber/messages/go/v34"
)

var typeTags = map[string]bool{
	"happy": true, "boundary": true, "negative": true, "error": true,
	"concurrency": true, "security": true, "regression": true,
}

var todoComment = regexp.MustCompile(`^\s*#\s*todo:\s*(.*?)\s*$`)

// skipDirs are never scanned for .feature files.
var skipDirs = map[string]bool{".git": true, "node_modules": true, "vendor": true, "tmp": true, "public": true}

type entry struct {
	Feature  string `json:"feature"`
	Scenario string `json:"scenario"`
	Examples string `json:"examples"`
	URI      string `json:"uri"`
	Line     int64  `json:"line"`
	Type     string `json:"type"`
	Status   string `json:"status"`
	Note     string `json:"note"`

	pickleNames []string
}

type testEvent struct {
	Action string
	Test   string
}

func main() {
	if len(os.Args) != 2 {
		fmt.Fprintln(os.Stderr, "usage: normalize_scenarios <go-test.json>")
		os.Exit(1)
	}
	if err := run(os.Args[1]); err != nil {
		fmt.Fprintln(os.Stderr, "normalize_scenarios:", err)
		os.Exit(1)
	}
}

func run(eventsPath string) error {
	results, err := readResults(eventsPath)
	if err != nil {
		return err
	}
	var entries []*entry
	err = filepath.WalkDir(".", func(path string, d fs.DirEntry, err error) error {
		if err != nil {
			return err
		}
		if d.IsDir() && skipDirs[d.Name()] {
			return filepath.SkipDir
		}
		if d.IsDir() || !strings.HasSuffix(path, ".feature") {
			return nil
		}
		fileEntries, err := parseFeature(filepath.ToSlash(path))
		if err != nil {
			return fmt.Errorf("%s: %w", path, err)
		}
		entries = append(entries, fileEntries...)
		return nil
	})
	if err != nil {
		return err
	}
	for _, e := range entries {
		if e.Status != "todo" {
			e.Status = statusOf(e.pickleNames, results)
		}
	}
	if entries == nil {
		entries = []*entry{}
	}
	if err := os.MkdirAll("tmp/result", 0o755); err != nil {
		return err
	}
	data, err := json.MarshalIndent(struct {
		Scenarios []*entry `json:"scenarios"`
	}{entries}, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile("tmp/result/scenarios.json", data, 0o644)
}

// readResults maps a subtest name below TestFeatures (e.g. "Check_a_URL#01")
// to every final pass/fail/skip action reported for it, across packages.
func readResults(path string) (map[string][]string, error) {
	f, err := os.Open(path)
	if err != nil {
		return nil, err
	}
	defer f.Close()
	results := map[string][]string{}
	sc := bufio.NewScanner(f)
	sc.Buffer(make([]byte, 64*1024), 1024*1024)
	for sc.Scan() {
		var ev testEvent
		if json.Unmarshal(sc.Bytes(), &ev) != nil {
			continue
		}
		name, ok := strings.CutPrefix(ev.Test, "TestFeatures/")
		if !ok {
			continue
		}
		switch ev.Action {
		case "pass", "fail", "skip":
			results[name] = append(results[name], ev.Action)
		}
	}
	return results, sc.Err()
}

// statusOf is "failed" if any run of any pickle failed, "missing" if a
// pickle has no pass/fail result (never run, or skipped), else "passed".
func statusOf(pickleNames []string, results map[string][]string) string {
	status := "passed"
	for _, name := range pickleNames {
		base := rewrite(name)
		var actions []string
		for key, acts := range results {
			if key == base || (strings.HasPrefix(key, base+"#") && isDigits(key[len(base)+1:])) {
				actions = append(actions, acts...)
			}
		}
		passedOrFailed := false
		for _, a := range actions {
			switch a {
			case "fail":
				return "failed"
			case "pass":
				passedOrFailed = true
			}
		}
		if !passedOrFailed {
			status = "missing"
		}
	}
	return status
}

// rewrite mirrors how `go test` turns a t.Run name into a test name.
func rewrite(s string) string {
	var b strings.Builder
	for _, r := range s {
		switch {
		case unicode.IsSpace(r):
			b.WriteRune('_')
		case !unicode.IsPrint(r):
			b.WriteString(strings.Trim(fmt.Sprintf("%q", r), `'`))
		default:
			b.WriteRune(r)
		}
	}
	return b.String()
}

func isDigits(s string) bool {
	if s == "" {
		return false
	}
	for _, r := range s {
		if r < '0' || r > '9' {
			return false
		}
	}
	return true
}

func parseFeature(uri string) ([]*entry, error) {
	f, err := os.Open(uri)
	if err != nil {
		return nil, err
	}
	defer f.Close()
	newID := (&messages.Incrementing{}).NewId
	doc, err := gherkin.ParseGherkinDocument(f, newID)
	if err != nil {
		return nil, err
	}
	if doc.Feature == nil {
		return nil, nil
	}
	notes := map[int64]string{} // line -> "# todo:" reason
	for _, c := range doc.Comments {
		if m := todoComment.FindStringSubmatch(c.Text); m != nil {
			notes[c.Location.Line] = m[1]
		}
	}
	noteAbove := func(tags []*messages.Tag, loc *messages.Location) string {
		top := loc.Line
		for _, t := range tags {
			if t.Location.Line < top {
				top = t.Location.Line
			}
		}
		return notes[top-1]
	}

	feature := scope{doc.Feature.Tags, noteAbove(doc.Feature.Tags, doc.Feature.Location)}

	var entries []*entry
	byScenario := map[string]*entry{} // scenario AST id -> entry (plain scenario)
	byRow := map[string]*entry{}      // examples row AST id -> entry (outline block)

	addScenario := func(sc *messages.Scenario, parents []scope) {
		own := scope{sc.Tags, noteAbove(sc.Tags, sc.Location)}
		chain := append(append([]scope{}, parents...), own)
		if len(sc.Examples) == 0 {
			e := newEntry(doc.Feature.Name, sc.Name, "", uri, sc.Location.Line, chain)
			byScenario[sc.Id] = e
			entries = append(entries, e)
			return
		}
		for _, ex := range sc.Examples {
			block := append(append([]scope{}, chain...), scope{ex.Tags, noteAbove(ex.Tags, ex.Location)})
			e := newEntry(doc.Feature.Name, sc.Name, ex.Name, uri, ex.Location.Line, block)
			for _, row := range ex.TableBody {
				byRow[row.Id] = e
			}
			entries = append(entries, e)
		}
	}
	for _, child := range doc.Feature.Children {
		if child.Scenario != nil {
			addScenario(child.Scenario, []scope{feature})
		}
		if child.Rule != nil {
			rule := scope{child.Rule.Tags, noteAbove(child.Rule.Tags, child.Rule.Location)}
			for _, rc := range child.Rule.Children {
				if rc.Scenario != nil {
					addScenario(rc.Scenario, []scope{feature, rule})
				}
			}
		}
	}

	for _, p := range gherkin.Pickles(*doc, uri, newID) {
		var e *entry
		if len(p.AstNodeIds) > 1 {
			e = byRow[p.AstNodeIds[1]]
		} else {
			e = byScenario[p.AstNodeIds[0]]
		}
		if e != nil {
			e.pickleNames = append(e.pickleNames, p.Name)
		}
	}
	return entries, nil
}

// scope is one tagged Gherkin level (Feature, Rule, Scenario, Examples) with
// the "# todo:" reason written directly above it.
type scope struct {
	tags []*messages.Tag
	note string
}

// newEntry derives type, @todo status, and note from the entry's tag scopes,
// outermost (feature) first.
func newEntry(feature, scenario, examples, uri string, line int64, chain []scope) *entry {
	e := &entry{Feature: feature, Scenario: scenario, Examples: examples, URI: uri, Line: line}
	types := map[string]bool{}
	for _, sc := range chain {
		for _, t := range sc.tags {
			name := strings.TrimPrefix(t.Name, "@")
			if typeTags[name] {
				types[name] = true
			}
			if name == "todo" {
				e.Status = "todo"
				if e.Note == "" {
					e.Note = sc.note
				}
			}
		}
	}
	e.Type = "untyped"
	if len(types) == 1 {
		for t := range types {
			e.Type = t
		}
	}
	return e
}
