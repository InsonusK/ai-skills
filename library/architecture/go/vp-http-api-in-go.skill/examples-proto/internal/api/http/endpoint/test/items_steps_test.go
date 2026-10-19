package test

import (
	"context"
	"fmt"
	"net/http"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)"$`, w.planReturns)
	sc.Step(`^AddItem returns the item "([^"]*)"$`, w.addReturns)
	sc.Step(`^AddItem fails with the kind conflict and "(.*)"$`, w.addConflicts)
	sc.Step(`^ListItems returns the items "([^"]*)"$`, w.listReturns)
	sc.Step(`^I POST "([^"]*)" with the body '([^']*)'$`, w.iPOST)
	sc.Step(`^I GET "([^"]*)"$`, w.iGET)
	sc.Step(`^the response status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
	sc.Step(`^PlanItems should have received the titles "([^"]*)"$`, w.planShouldHaveReceived)
	sc.Step(`^AddItem should have received the list "([^"]*)" and the title "([^"]*)"$`, w.addShouldHaveReceived)
	sc.Step(`^ListItems should have received the list "([^"]*)"$`, w.listShouldHaveReceived)
}

func views(titles string) []command.ItemView {
	items := []command.ItemView{}
	for _, title := range strings.Split(titles, "|") {
		items = append(items, command.ItemView{Title: title})
	}
	return items
}

func (w *World) planReturns(ctx context.Context, titles string) error {
	w.planResult = command.PlanItemsResult{Items: views(titles)}
	return nil
}

func (w *World) addReturns(ctx context.Context, title string) error {
	w.addResult = command.AddItemResult{Item: command.ItemView{Title: title}}
	return nil
}

func (w *World) addConflicts(ctx context.Context, message string) error {
	w.addErr = fmt.Errorf("%w: %s", pipeline.ErrConflict, message)
	return nil
}

func (w *World) listReturns(ctx context.Context, titles string) error {
	w.listResult = command.ListItemsResult{Items: views(titles)}
	return nil
}

func (w *World) iPOST(ctx context.Context, path, body string) error {
	w.send(http.MethodPost, path, body)
	return nil
}

func (w *World) iGET(ctx context.Context, path string) error {
	w.send(http.MethodGet, path, "")
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

func (w *World) addShouldHaveReceived(ctx context.Context, listID, title string) error {
	if w.addGot != (command.AddItem{ListID: listID, Title: title}) {
		return fmt.Errorf("AddItem received %+v, want list %q title %q", w.addGot, listID, title)
	}
	return nil
}

func (w *World) listShouldHaveReceived(ctx context.Context, listID string) error {
	if w.listGot.ListID != listID {
		return fmt.Errorf("ListItems received %+v, want list %q", w.listGot, listID)
	}
	return nil
}
