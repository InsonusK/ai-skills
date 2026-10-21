package test

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/cucumber/godog"
)

func registerCallLogSteps(sc *godog.ScenarioContext, w *World) {
	var sentID string
	sc.Step(`^I call the test method with "([^"]*)" and the request id "([^"]*)"$`, func(text, id string) error {
		conn, err := w.client()
		if err != nil {
			return err
		}
		w.reply, sentID, w.callErr = callEchoWithID(conn, text, id)
		return nil
	})
	sc.Step(`^the server should send back the request id "([^"]*)"$`, func(want string) error {
		if sentID != want {
			return fmt.Errorf("request id sent back: got %q, want %q", sentID, want)
		}
		return nil
	})
	sc.Step(`^the context of the command should carry the request id "([^"]*)"$`, func(want string) error {
		if w.echoReqID != want {
			return fmt.Errorf("request id in the command's context: got %q, want %q", w.echoReqID, want)
		}
		return nil
	})
	sc.Step(`^one "([^"]*)" record should be logged with the method "([^"]*)", the code "([^"]*)", and failed "([^"]*)"$`,
		func(msg, method, code, failed string) error {
			recs := w.records(msg)
			if len(recs) != 1 {
				return fmt.Errorf("%q records: got %d, want 1 (log %s)", msg, len(recs), w.logs.String())
			}
			r := recs[0]
			if r["method"] != method || r["code"] != code || fmt.Sprint(r["failed"]) != failed {
				return fmt.Errorf("record %v, want method %s code %s failed %s", r, method, code, failed)
			}
			return nil
		})
	sc.Step(`^its payload should be the message "([^"]*)"$`, func(want string) error {
		recs := w.records("grpc call")
		if len(recs) != 1 {
			return fmt.Errorf("grpc call records: got %d, want 1", len(recs))
		}
		// the JSON handler writes a generated message through encoding/json: its exported fields
		payload, _ := recs[0]["payload"].(map[string]any)
		if payload["value"] != want {
			return fmt.Errorf("payload %v, want the message %q", recs[0]["payload"], want)
		}
		return nil
	})
}

// records are the logged records of the given msg.
func (w *World) records(msg string) []map[string]any {
	found := []map[string]any{}
	for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
		var rec map[string]any
		if json.Unmarshal([]byte(line), &rec) == nil && rec["msg"] == msg {
			found = append(found, rec)
		}
	}
	return found
}
