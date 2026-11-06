package test

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/shared/errorkind"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)"$`, w.planReturns)
	sc.Step(`^AddItem returns the item "([^"]*)"$`, w.addReturns)
	sc.Step(`^AddItem fails with the kind "([^"]*)" and '([^']*)'$`, w.addFailsWithKind)
	sc.Step(`^the HTTP API serves the application's routes$`, func() error { return nil }) // every scenario here does
	sc.Step(`^the HTTP error should have code (\d+) and the message ["'](.*)["']$`, w.theErrorShouldHave)
	sc.Step(`^PlanItems should have received no titles$`, w.planShouldHaveReceivedNoTitles)
	sc.Step(`^ListItems returns the items "([^"]*)"$`, w.listReturns)
	sc.Step(`^I POST "([^"]*)" to the HTTP API with the body '([^']*)'$`, w.iPOST)
	sc.Step(`^I GET "([^"]*)" from the HTTP API$`, w.iGET)
	sc.Step(`^the HTTP status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the HTTP response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
	sc.Step(`^PlanItems should have received the titles "([^"]*)"$`, w.planShouldHaveReceived)
	sc.Step(`^AddItem should have received the list "([^"]*)" and the title "([^"]*)"$`, w.addShouldHaveReceived)
	sc.Step(`^ListItems should have received the list "([^"]*)"$`, w.listShouldHaveReceived)
}

func views(titles string) []command.ItemView {
	items := []command.ItemView{}
	if titles == "" {
		return items
	}
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

func (w *World) addFailsWithKind(ctx context.Context, name, message string) error {
	for _, kind := range errorkind.Kinds {
		if kind.Error() == name {
			w.addErr = errorkind.New(kind, message)
			return nil
		}
	}
	return fmt.Errorf("no kind %q", name)
}

func (w *World) theErrorShouldHave(ctx context.Context, code int, message string) error {
	var body struct {
		Code    int    `json:"code"`
		Message string `json:"message"`
	}
	if err := json.Unmarshal(w.response.Body.Bytes(), &body); err != nil || body.Code != code || body.Message != message {
		return fmt.Errorf("error body %s, want code %d and %q", w.response.Body.String(), code, message)
	}
	return nil
}

func (w *World) planShouldHaveReceivedNoTitles(ctx context.Context) error {
	if !w.planCalled || len(w.planGot.Titles) != 0 {
		return fmt.Errorf("handler received %q (called=%v), want no titles", w.planGot.Titles, w.planCalled)
	}
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
	if w.addGot != (command.AddItemCommand{ListID: listID, Title: title}) {
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
