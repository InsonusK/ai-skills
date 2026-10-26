package test

import (
	"context"
	"errors"
	"fmt"
	"reflect"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/domain/entity"
)

// fakePlanner is the remote planner faked: the titles it plans or the error
// it returns, and the titles it was given.
type fakePlanner struct {
	titles []string
	err    error
	given  []string
}

func (f *fakePlanner) Plan(_ context.Context, titles []string) ([]entity.Item, error) {
	f.given = titles
	if f.err != nil {
		return nil, f.err
	}
	items := make([]entity.Item, len(f.titles))
	for i, title := range f.titles {
		item, err := entity.NewItem(title)
		if err != nil {
			return nil, err
		}
		items[i] = item
	}
	return items, nil
}

var kinds = map[string]error{"invalid": entity.ErrInvalid, "unavailable": entity.ErrUnavailable}

func registerPlanRemoteItemsSteps(sc *godog.ScenarioContext) {
	var planner *fakePlanner
	var result command.PlanRemoteItemsResult
	var err error
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		planner, result, err = &fakePlanner{}, command.PlanRemoteItemsResult{}, nil
		return ctx, nil
	})
	sc.Step(`^the remote planner plans "([^"]*)"$`, func(titles string) error {
		planner.titles = strings.Split(titles, "|")
		return nil
	})
	sc.Step(`^the remote planner fails with the kind "([^"]*)"$`, func(kind string) error {
		planner.err = fmt.Errorf("%w: answered by the scenario", kinds[kind])
		return nil
	})
	sc.Step(`^I send PlanRemoteItems with the titles "([^"]*)"$`, func(titles string) error {
		result, err = command.NewPlanRemoteItemsHandler(planner).Handle(context.Background(), command.PlanRemoteItemsCommand{Titles: strings.Split(titles, "|")})
		logf("when: PlanRemoteItems %s -> %+v, error %v", titles, result, err)
		return nil
	})
	sc.Step(`^the remote result should list the open items "([^"]*)"$`, func(titles string) error {
		var want []command.ItemView
		for _, title := range strings.Split(titles, "|") {
			want = append(want, command.ItemView{Title: title})
		}
		if err != nil || !reflect.DeepEqual(result.Items, want) {
			return fmt.Errorf("got %+v, error %v; want %+v", result.Items, err, want)
		}
		return nil
	})
	sc.Step(`^the remote planner should have been given "([^"]*)"$`, func(titles string) error {
		if !reflect.DeepEqual(planner.given, strings.Split(titles, "|")) {
			return fmt.Errorf("given %v, want %s", planner.given, titles)
		}
		return nil
	})
	sc.Step(`^the command should fail with the kind "([^"]*)"$`, func(kind string) error {
		if !errors.Is(err, kinds[kind]) {
			return fmt.Errorf("error %v, want the kind %s", err, kind)
		}
		return nil
	})
}
