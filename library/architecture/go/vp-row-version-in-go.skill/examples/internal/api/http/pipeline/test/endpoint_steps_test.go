package test

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/shared/errorkind"
)

func registerAPISteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the echo command answers "([^"]*)"$`, w.commandAnswers)
	sc.Step(`^the echo command is rejected with "([^"]*)"$`, w.commandRejected)
	sc.Step(`^the echo command fails with "([^"]*)"$`, w.commandFails)
	sc.Step(`^the echo command fails with the kind "([^"]*)" and "([^"]*)"$`, w.commandFailsWithKind)
	sc.Step(`^the echo command fails with the kind "([^"]*)" and "([^"]*)", wrapped in "([^"]*)"$`, w.commandFailsWrapped)
	sc.Step(`^I POST "([^"]*)" to the HTTP API with the body '([^']*)' as "([^"]*)"$`, w.iPOSTAs)
	sc.Step(`^the HTTP log should hold the error "([^"]*)" of "([^"]*)"$`, w.theLogShouldHoldTheError)
	sc.Step(`^I POST "([^"]*)" to the HTTP API with the body '([^']*)'$`, w.iPOST)
	sc.Step(`^I GET "([^"]*)" from the HTTP API$`, w.iGET)
	sc.Step(`^the HTTP status should be (\d+)$`, w.theResponseStatusShouldBe)
	sc.Step(`^the HTTP response body should be '([^']*)'$`, w.theResponseBodyShouldBe)
	sc.Step(`^the HTTP response body should mention "([^"]*)"$`, w.theResponseBodyShouldMention)
	sc.Step(`^the echo command should have received "([^"]*)"$`, w.commandShouldHaveReceived)
	sc.Step(`^the echo command should not have been called$`, w.commandShouldNotHaveBeenCalled)
	sc.Step(`^the HTTP error should have code (\d+) and a message containing "([^"]*)"$`, w.errorBodyContaining)
	sc.Step(`^the HTTP error should have code (\d+) and the message "([^"]*)"$`, w.errorBodyExactly)
}

func (w *World) commandAnswers(ctx context.Context, text string) error {
	w.echoAnswer, w.echoErr, w.echoPanics = text, nil, false
	return nil
}

func (w *World) commandRejected(ctx context.Context, message string) error {
	w.echoErr = errRejected(message)
	return nil
}

func (w *World) commandFails(ctx context.Context, message string) error {
	w.echoErr = errors.New(message)
	return nil
}

// kindNamed is the kind the scenarios name by its text ("not found").
func kindNamed(name string) (error, error) {
	for _, kind := range errorkind.Kinds {
		if kind.Error() == name {
			return kind, nil
		}
	}
	return nil, fmt.Errorf("no kind %q", name)
}

func (w *World) commandFailsWithKind(ctx context.Context, name, message string) error {
	kind, err := kindNamed(name)
	w.echoErr = errorkind.New(kind, message)
	return err
}

func (w *World) commandFailsWrapped(ctx context.Context, name, message, context string) error {
	kind, err := kindNamed(name)
	w.echoErr = fmt.Errorf("%s: %w", context, errorkind.New(kind, message))
	return err
}

func (w *World) iPOSTAs(ctx context.Context, path, body, contentType string) error {
	w.sendWithHeaders(http.MethodPost, path, body, map[string]string{"Content-Type": contentType})
	return nil
}

func (w *World) theLogShouldHoldTheError(ctx context.Context, text, msg string) error {
	for _, rec := range w.records(msg) {
		if rec["error"] == text {
			return nil
		}
	}
	return fmt.Errorf("no %q record with the error %q in %s", msg, text, w.logs.String())
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

func (w *World) commandShouldHaveReceived(ctx context.Context, text string) error {
	if !w.echoCalled || w.echoGot.Text != text {
		return fmt.Errorf("handler received %q (called=%v), want %q", w.echoGot.Text, w.echoCalled, text)
	}
	return nil
}

func (w *World) commandShouldNotHaveBeenCalled(ctx context.Context) error {
	if w.echoCalled {
		return fmt.Errorf("handler was called with %+v", w.echoGot)
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
