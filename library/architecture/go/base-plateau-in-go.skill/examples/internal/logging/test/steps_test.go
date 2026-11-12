package test

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"log/slog"
	"regexp"
	"strings"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/logging"
	"github.com/example/todo/internal/shared/errorkind"
)

// kindNamed is the kind the scenarios name by its text ("unavailable").
func kindNamed(name string) (error, error) {
	for _, kind := range errorkind.Kinds {
		if kind.Error() == name {
			return kind, nil
		}
	}
	return nil, fmt.Errorf("no kind %q", name)
}

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
	logError := func(msg string, err error) {
		// A record of a failed call, as the command logging middleware writes it.
		logger := slog.New(logging.NewHandler(&out, level))
		logger.LogAttrs(context.Background(), slog.LevelWarn, msg, logging.Error(err))
		fmt.Printf("when: logged -> %s", out.String())
	}
	sc.Step(`^a record "([^"]*)" is logged with a duration of (\d+) milliseconds$`, func(msg string, ms int) error {
		logger := slog.New(logging.NewHandler(&out, level))
		logger.LogAttrs(context.Background(), slog.LevelInfo, msg, slog.Duration("duration", time.Duration(ms)*time.Millisecond))
		fmt.Printf("when: logged -> %s", out.String())
		return nil
	})
	sc.Step(`^a record "([^"]*)" is logged with an error "([^"]*)" of the kind "([^"]*)"$`, func(msg, text, name string) error {
		kind, err := kindNamed(name)
		if err != nil {
			return err
		}
		logError(msg, errorkind.New(kind, text))
		return nil
	})
	sc.Step(`^a record "([^"]*)" is logged with an error "([^"]*)" of the kind "([^"]*)" caused by "([^"]*)"$`, func(msg, text, name, cause string) error {
		kind, err := kindNamed(name)
		if err != nil {
			return err
		}
		logError(msg, errorkind.Wrap(kind, text, errors.New(cause)))
		return nil
	})
	line := func() (map[string]any, error) {
		var rec map[string]any
		err := json.Unmarshal(out.Bytes(), &rec)
		return rec, err
	}
	sc.Step(`^the line should have "([^"]*)" = "([^"]*)"$`, func(key, want string) error {
		rec, err := line()
		got, isText := rec[key].(string)
		if !isText {
			raw, _ := json.Marshal(rec[key])
			got = string(raw)
		}
		if err != nil || got != want {
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
	sc.Step(`^the request id "([^"]*)" is begun$`, func(id string) error {
		ctx = logging.WithRequestID(ctx, id)
		return nil
	})
	sc.Step(`^the current request id should be "([^"]*)"$`, func(want string) error {
		if got := logging.RequestID(ctx); got != want {
			return fmt.Errorf("request id: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the current request id should be 16 hex characters$`, func() error {
		if got := logging.RequestID(ctx); !regexp.MustCompile(`^[0-9a-f]{16}$`).MatchString(got) {
			return fmt.Errorf("request id: got %q, want 16 hex characters", got)
		}
		return nil
	})
}
