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
	sc.Step(`^I POST "([^"]*)" to the HTTP API with the body '([^']*)'$`, w.iPOST)
	sc.Step(`^the HTTP status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the HTTP response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
	sc.Step(`^PlanItems should have received the titles "([^"]*)"$`, w.planShouldHaveReceived)
	sc.Step(`^PlanItems should have received no titles$`, w.planShouldHaveReceivedNoTitles)
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

func (w *World) planShouldHaveReceivedNoTitles(ctx context.Context) error {
	if !w.planCalled || len(w.planGot.Titles) != 0 {
		return fmt.Errorf("handler received %q (called=%v), want no titles", w.planGot.Titles, w.planCalled)
	}
	return nil
}
