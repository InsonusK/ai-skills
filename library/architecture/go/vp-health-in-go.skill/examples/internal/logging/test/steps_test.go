package test

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"log/slog"
	"regexp"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/logging"
)

func registerSteps(sc *godog.ScenarioContext) {
	var out bytes.Buffer
	level := slog.LevelInfo
	var ctx context.Context
	sc.Before(func(c context.Context, _ *godog.Scenario) (context.Context, error) {
		out.Reset()
		level, ctx = slog.LevelInfo, context.Background()
		return c, nil
	})
	sc.Step(`^the level is "([^"]*)"$`, func(name string) error { return level.UnmarshalText([]byte(name)) })
	sc.Step(`^a record "([^"]*)" is logged with the command "([^"]*)" and the payload "([^"]*)"$`, func(msg, command, payload string) error {
		logger := slog.New(logging.NewHandler(&out, level))
		logger.LogAttrs(context.Background(), slog.LevelInfo, msg, slog.String("command", command), logging.Payload(payload))
		fmt.Printf("when: logged -> %s", out.String())
		return nil
	})
	line := func() (map[string]any, error) {
		var rec map[string]any
		err := json.Unmarshal(out.Bytes(), &rec)
		return rec, err
	}
	sc.Step(`^the line should have "([^"]*)" = "([^"]*)"$`, func(key, want string) error {
		rec, err := line()
		if err != nil || rec[key] != want {
			return fmt.Errorf("line %s (err %v): %s is %v, want %q", out.String(), err, key, rec[key], want)
		}
		return nil
	})
	sc.Step(`^the line should have no "([^"]*)"$`, func(key string) error {
		rec, err := line()
		if _, has := rec[key]; err != nil || has {
			return fmt.Errorf("line %s (err %v) has %q", out.String(), err, key)
		}
		return nil
	})
	sc.Step(`^the line should not mention "([^"]*)"$`, func(text string) error {
		if strings.Contains(out.String(), text) {
			return fmt.Errorf("line %s mentions %q", out.String(), text)
		}
		return nil
	})
	sc.Step(`^nothing should be written$`, func() error {
		if out.Len() > 0 {
			return fmt.Errorf("written: %s", out.String())
		}
		return nil
	})
	sc.Step(`^the request id "([^"]*)" is put into the context$`, func(id string) error {
		ctx = logging.WithRequestID(ctx, id)
		return nil
	})
	sc.Step(`^the request id of the context should be "([^"]*)"$`, func(want string) error {
		if got := logging.RequestID(ctx); got != want {
			return fmt.Errorf("request id: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the request id of the context should be 16 hex characters$`, func() error {
		if got := logging.RequestID(ctx); !regexp.MustCompile(`^[0-9a-f]{16}$`).MatchString(got) {
			return fmt.Errorf("request id: got %q, want 16 hex characters", got)
		}
		return nil
	})
}
