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
	payloadOf := func(msg string) (map[string]any, error) {
		recs := w.records(msg)
		if len(recs) != 1 {
			return nil, fmt.Errorf("%q records: got %d, want 1 (log %s)", msg, len(recs), w.logs.String())
		}
		payload, _ := recs[0]["payload"].(map[string]any)
		return payload, nil
	}
	// the JSON handler writes a generated message through encoding/json: its exported fields
	value := func(part any) any {
		m, _ := part.(map[string]any)
		return m["value"]
	}
	// the records of the call, in order; the server's own (listening, stopped) are left out
	sc.Step(`^the records should be, in order, "([^"]*)"$`, func(msgs string) error {
		got := []string{}
		for _, line := range strings.Split(strings.TrimSpace(w.logs.String()), "\n") {
			var rec map[string]any
			if json.Unmarshal([]byte(line), &rec) == nil && strings.Contains(msgs, fmt.Sprint(rec["msg"])) {
				got = append(got, fmt.Sprint(rec["msg"]))
			}
		}
		if strings.Join(got, "|") != msgs {
			return fmt.Errorf("records in order: got %q, want %q", strings.Join(got, "|"), msgs)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the request "([^"]*)"$`, func(msg, want string) error {
		payload, err := payloadOf(msg)
		if err != nil {
			return err
		}
		if value(payload["request"]) != want {
			return fmt.Errorf("payload %v, want the request %q", payload, want)
		}
		return nil
	})
	sc.Step(`^the "([^"]*)" record should hold the response "([^"]*)" and no request$`, func(msg, want string) error {
		payload, err := payloadOf(msg)
		if err != nil {
			return err
		}
		if _, has := payload["request"]; has || value(payload["response"]) != want {
			return fmt.Errorf("payload %v, want the response %q and no request", payload, want)
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
