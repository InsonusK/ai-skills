package test

import (
	"context"
	"fmt"
	"strings"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/gen/api/todo"
	"github.com/example/todo/internal/command"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)" over gRPC$`, w.planReturns)
	sc.Step(`^I call ItemsService.PlanItems with the titles "([^"]*)"$`, w.iCallPlanItems)
	sc.Step(`^I call ItemsService.PlanItems with no titles$`, w.iCallPlanItemsWithNoTitles)
	sc.Step(`^the gRPC response should hold the items "([^"]*)", none done$`, w.theResponseShouldHold)
	sc.Step(`^PlanItems should have received the titles "([^"]*)" over gRPC$`, w.planShouldHaveReceived)
	sc.Step(`^PlanItems should have received no titles over gRPC$`, w.planShouldHaveReceivedNoTitles)
}

func (w *World) planReturns(ctx context.Context, titles string) error {
	w.planResult = command.PlanItemsResult{}
	if titles == "" {
		return nil
	}
	for _, title := range strings.Split(titles, "|") {
		w.planResult.Items = append(w.planResult.Items, command.ItemView{Title: title})
	}
	return nil
}

func (w *World) iCallPlanItems(ctx context.Context, titles string) error {
	conn, err := w.client()
	if err != nil {
		return err
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	w.planResponse, err = todo.NewItemsServiceClient(conn).PlanItems(ctx, &todo.PlanItemsRequest{Titles: strings.Split(titles, "|")})
	logf("when: PlanItems(%q) -> %v err=%v", titles, w.planResponse, err)
	return err
}

func (w *World) iCallPlanItemsWithNoTitles(ctx context.Context) error {
	conn, err := w.client()
	if err != nil {
		return err
	}
	ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
	defer cancel()
	w.planResponse, err = todo.NewItemsServiceClient(conn).PlanItems(ctx, &todo.PlanItemsRequest{})
	logf("when: PlanItems() -> %v err=%v", w.planResponse, err)
	return err
}

func (w *World) planShouldHaveReceivedNoTitles(ctx context.Context) error {
	if !w.planCalled || len(w.planGot.Titles) != 0 {
		return fmt.Errorf("handler received %q (called=%v), want no titles", w.planGot.Titles, w.planCalled)
	}
	return nil
}

func (w *World) theResponseShouldHold(ctx context.Context, titles string) error {
	got := []string{}
	for _, item := range w.planResponse.GetItems() {
		if item.GetDone() {
			return fmt.Errorf("item %q is done", item.GetTitle())
		}
		got = append(got, item.GetTitle())
	}
	if strings.Join(got, "|") != titles {
		return fmt.Errorf("items: got %q, want %q", strings.Join(got, "|"), titles)
	}
	return nil
}

func (w *World) planShouldHaveReceived(ctx context.Context, titles string) error {
	if got := strings.Join(w.planGot.Titles, "|"); !w.planCalled || got != titles {
		return fmt.Errorf("handler received %q (called=%v), want %q", got, w.planCalled, titles)
	}
	return nil
}
