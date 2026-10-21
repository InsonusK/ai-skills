package test

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/cucumber/godog"
)

func registerCallLogSteps(sc *godog.ScenarioContext, w *World) {
	record := func() (map[string]any, error) {
		found := []map[string]any{}
		for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
			var rec map[string]any
			if json.Unmarshal([]byte(line), &rec) == nil && rec["msg"] == "cli call" {
				found = append(found, rec)
			}
		}
		if len(found) != 1 {
			return nil, fmt.Errorf(`"cli call" records: got %d, want 1 (log %s)`, len(found), w.logs.String())
		}
		return found[0], nil
	}
	sc.Step(`^one "cli call" record should be logged for the action "([^"]*)", failed "([^"]*)"$`, func(action, failed string) error {
		rec, err := record()
		if err != nil {
			return err
		}
		if rec["action"] != action || fmt.Sprint(rec["failed"]) != failed {
			return fmt.Errorf("record %v, want action %q failed %s", rec, action, failed)
		}
		return nil
	})
	sc.Step(`^its payload should hold the output "([^"]*)"$`, func(want string) error {
		rec, err := record()
		if err != nil {
			return err
		}
		payload, _ := rec["payload"].(map[string]any)
		if got := strings.TrimSpace(fmt.Sprint(payload["output"])); got != want {
			return fmt.Errorf("output in the payload: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^its payload should hold the flag "([^"]*)" = "([^"]*)"$`, func(name, value string) error {
		rec, err := record()
		if err != nil {
			return err
		}
		payload, _ := rec["payload"].(map[string]any)
		flags, _ := payload["flags"].(map[string]any)
		if flags[name] != value {
			return fmt.Errorf("payload %v, want %s=%s", rec["payload"], name, value)
		}
		return nil
	})
}
