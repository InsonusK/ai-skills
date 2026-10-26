package test

import (
	"context"
	"errors"
	"fmt"
	"reflect"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/domain/entity"
	"github.com/example/todo/internal/infrastructure/httpclient"
	"github.com/example/todo/internal/infrastructure/todoapi"
	"github.com/example/todo/internal/service"
)

func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// fakeTodoAPI is the adapter faked: the items it answers or the technical
// outcome it reports.
type fakeTodoAPI struct {
	items []todoapi.Item
	err   error
}

func (f *fakeTodoAPI) PlanItems(_ context.Context, _ []string) ([]todoapi.Item, error) {
	return f.items, f.err
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	api   fakeTodoAPI
	items []entity.Item
	err   error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
}

var kinds = map[string]error{"invalid": entity.ErrInvalid, "unavailable": entity.ErrUnavailable}

func registerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the todo API plans "([^"]*)"$`, func(titles string) error {
		for _, title := range strings.Split(titles, "|") {
			w.api.items = append(w.api.items, todoapi.Item{Title: title})
		}
		return nil
	})
	sc.Step(`^the todo API is unavailable$`, func() error {
		w.api.err = fmt.Errorf("%w: connection refused", httpclient.ErrUnavailable)
		return nil
	})
	sc.Step(`^the todo API answers (\d+) with the code (\d+) and the message "([^"]*)"$`, func(status, code int, message string) error {
		w.api.err = &httpclient.StatusError{HTTPStatus: status, Code: code, Message: message}
		return nil
	})
	sc.Step(`^I plan "([^"]*)" remotely$`, func(titles string) error {
		w.items, w.err = service.NewRemotePlanner(&w.api).Plan(context.Background(), strings.Split(titles, "|"))
		logf("when: Plan %s -> %d items, error %v", titles, len(w.items), w.err)
		return nil
	})
	sc.Step(`^the planned items should be the open items "([^"]*)"$`, func(titles string) error {
		var got []string
		for _, item := range w.items {
			if item.Done() {
				return fmt.Errorf("%q is done", item.Title())
			}
			got = append(got, item.Title())
		}
		if w.err != nil || !reflect.DeepEqual(got, strings.Split(titles, "|")) {
			return fmt.Errorf("got %v, error %v; want %s", got, w.err, titles)
		}
		return nil
	})
	sc.Step(`^planning should fail with the kind "([^"]*)"$`, func(kind string) error {
		if !errors.Is(w.err, kinds[kind]) {
			return fmt.Errorf("error %v, want the kind %s", w.err, kind)
		}
		return nil
	})
	sc.Step(`^planning should fail with the kind "([^"]*)" mentioning "([^"]*)"$`, func(kind, text string) error {
		if !errors.Is(w.err, kinds[kind]) || !strings.Contains(w.err.Error(), text) {
			return fmt.Errorf("error %v, want the kind %s mentioning %q", w.err, kind, text)
		}
		return nil
	})
	sc.Step(`^planning should fail with no kind$`, func() error {
		if w.err == nil {
			return fmt.Errorf("planning succeeded")
		}
		for name, kind := range kinds {
			if errors.Is(w.err, kind) {
				return fmt.Errorf("error %v has the kind %s", w.err, name)
			}
		}
		return nil
	})
}
