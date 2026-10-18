package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
)

func registerAPISteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the PlanItems handler returns the items "([^"]*)"$`, w.planReturns)
	sc.Step(`^the PlanItems handler rejects the command with "([^"]*)"$`, w.planRejects)
	sc.Step(`^the PlanItems handler fails with "([^"]*)"$`, w.planFails)
	sc.Step(`^I POST "([^"]*)" with the body '([^']*)'$`, w.iPOST)
	sc.Step(`^I GET "([^"]*)"$`, w.iGET)
	sc.Step(`^the response status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
	sc.Step(`^the response body should mention "([^"]*)"$`, w.theResponseBodyShouldMention)
	sc.Step(`^the PlanItems handler should have received the titles "([^"]*)"$`, w.planShouldHaveReceived)
	sc.Step(`^the PlanItems handler should not have been called$`, w.planShouldNotHaveBeenCalled)
	sc.Step(`^the error body should have code (\d+) and a message containing "([^"]*)"$`, w.errorBodyContaining)
	sc.Step(`^the error body should have code (\d+) and the message "([^"]*)"$`, w.errorBodyExactly)
}

func (w *World) planReturns(ctx context.Context, titles string) error {
	w.planResult = command.PlanItemsResult{}
	for _, title := range strings.Split(titles, "|") {
		w.planResult.Items = append(w.planResult.Items, command.ItemView{Title: title})
	}
	return nil
}

func (w *World) planRejects(ctx context.Context, message string) error {
	w.planErr = errRejected(message)
	return nil
}

func (w *World) planFails(ctx context.Context, message string) error {
	w.planErr = errors.New(message)
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

func (w *World) theResponseBodyShouldMention(ctx context.Context, text string) error {
	if !strings.Contains(w.response.Body.String(), text) {
		return fmt.Errorf("body does not mention %q", text)
	}
	return nil
}

func (w *World) planShouldHaveReceived(ctx context.Context, titles string) error {
	if got := strings.Join(w.planGot.Titles, "|"); !w.planCalled || got != titles {
		return fmt.Errorf("handler received %q (called=%v), want %q", got, w.planCalled, titles)
	}
	return nil
}

func (w *World) planShouldNotHaveBeenCalled(ctx context.Context) error {
	if w.planCalled {
		return fmt.Errorf("handler was called with %+v", w.planGot)
	}
	return nil
}

type statusBody struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
	Details []any  `json:"details"`
}

func (w *World) errorBody() (statusBody, error) {
	var body statusBody
	if err := json.Unmarshal(w.response.Body.Bytes(), &body); err != nil {
		return body, fmt.Errorf("error body is not google.rpc.Status JSON: %w", err)
	}
	if body.Details == nil {
		return body, fmt.Errorf("error body has no details array: %s", w.response.Body.String())
	}
	return body, nil
}

func (w *World) errorBodyContaining(ctx context.Context, code int, text string) error {
	body, err := w.errorBody()
	if err != nil {
		return err
	}
	if body.Code != code || !strings.Contains(body.Message, text) {
		return fmt.Errorf("error body: got %+v, want code %d with %q", body, code, text)
	}
	return nil
}

func (w *World) errorBodyExactly(ctx context.Context, code int, message string) error {
	body, err := w.errorBody()
	if err != nil {
		return err
	}
	if body.Code != code || body.Message != message {
		return fmt.Errorf("error body: got %+v, want code %d message %q", body, code, message)
	}
	return nil
}
