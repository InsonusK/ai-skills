package test

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/cucumber/godog"
)

func registerCallLogSteps(sc *godog.ScenarioContext, w *World) {
	all := func() []map[string]any {
		found := []map[string]any{}
		for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
			var rec map[string]any
			if json.Unmarshal([]byte(line), &rec) == nil {
				found = append(found, rec)
			}
		}
		return found
	}
	record := func(msg string) (map[string]any, error) {
		found := []map[string]any{}
		for _, rec := range all() {
			if rec["msg"] == msg {
				found = append(found, rec)
			}
		}
		if len(found) != 1 {
			return nil, fmt.Errorf("%q records: got %d, want 1 (log %s)", msg, len(found), w.logs.String())
		}
		return found[0], nil
	}
	payload := func(msg string) (map[string]any, error) {
		rec, err := record(msg)
		if err != nil {
			return nil, err
		}
		p, _ := rec["payload"].(map[string]any)
		return p, nil
	}
	sc.Step(`^the records should be, in order, "([^"]*)"$`, func(msgs string) error {
		got := []string{}
		for _, rec := range all() {
			got = append(got, fmt.Sprint(rec["msg"]))
		}
		if strings.Join(got, "|") != msgs {
			return fmt.Errorf("records in order: got %q, want %q", strings.Join(got, "|"), msgs)
		}
		return nil
	})
	sc.Step(`^one "([^"]*)" record should be logged for the action "([^"]*)", failed "([^"]*)"$`, func(msg, action, failed string) error {
		rec, err := record(msg)
		if err != nil {
			return err
		}
		if rec["action"] != action || fmt.Sprint(rec["failed"]) != failed {
			return fmt.Errorf("record %v, want action %q failed %s", rec, action, failed)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the flag "([^"]*)" = "([^"]*)"$`, func(msg, name, value string) error {
		p, err := payload(msg)
		if err != nil {
			return err
		}
		flags, _ := p["flags"].(map[string]any)
		if flags[name] != value {
			return fmt.Errorf("payload %v, want the flag %s=%s", p, name, value)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the output "([^"]*)"$`, func(msg, want string) error {
		p, err := payload(msg)
		if err != nil {
			return err
		}
		if got := strings.TrimSpace(fmt.Sprint(p["output"])); got != want {
			return fmt.Errorf("output in the payload: got %q, want %q", got, want)
		}
		return nil
	})
}
