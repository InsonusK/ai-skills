package test

import (
	"context"
	"fmt"
	"net/http"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)"$`, w.planReturns)
	sc.Step(`^I POST "([^"]*)" with the body '([^']*)'$`, w.iPOST)
	sc.Step(`^the response status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
	sc.Step(`^PlanItems should have received the titles "([^"]*)"$`, w.planShouldHaveReceived)
}

func (w *World) planReturns(ctx context.Context, titles string) error {
	w.planResult = command.PlanItemsResult{}
	for _, title := range strings.Split(titles, "|") {
		w.planResult.Items = append(w.planResult.Items, command.ItemView{Title: title})
	}
	return nil
}

func (w *World) iPOST(ctx context.Context, path, body string) error {
	w.send(http.MethodPost, path, body)
	return nil
}

func (w *World) theResponseStatusShouldBe(ctx context.Context, want int) error {
	if w.response.Code != want {
		return fmt.Errorf("status: got %d, want %d (body %s)", w.response.Code, want, w.response.Body.String())
	}
	return nil
}

func (w *World) theResponseBodyShouldBe(ctx context.Context, want string) error {
	if got := strings.TrimSpace(w.response.Body.String()); got != want {
		return fmt.Errorf("body: got %s, want %s", got, want)
	}
	return nil
}

func (w *World) planShouldHaveReceived(ctx context.Context, titles string) error {
	if got := strings.Join(w.planGot.Titles, "|"); !w.planCalled || got != titles {
		return fmt.Errorf("handler received %q (called=%v), want %q", got, w.planCalled, titles)
	}
	return nil
}
