package test

import (
	"context"
	"errors"
	"fmt"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"github.com/example/todo/internal/shared/errorkind"
)

func registerCallSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the gRPC echo command answers "([^"]*)"$`, w.commandAnswers)
	sc.Step(`^the gRPC echo command answers "([^"]*)", wrapped with logging$`, w.commandAnswersWithLogging)
	sc.Step(`^the gRPC echo command is rejected with "([^"]*)"$`, w.commandRejected)
	sc.Step(`^the gRPC echo command fails with "([^"]*)"$`, w.commandFails)
	sc.Step(`^the gRPC echo command fails with the kind "([^"]*)" and "([^"]*)"$`, w.commandFailsWithKind)
	sc.Step(`^the gRPC echo command fails with the kind "([^"]*)" and "([^"]*)", wrapped in "([^"]*)"$`, w.commandFailsWrapped)
	sc.Step(`^the gRPC echo command crashes$`, w.commandPanics)
	sc.Step(`^the gRPC echo command takes (\d+) milliseconds and answers "([^"]*)"$`, w.commandTakes)
	sc.Step(`^I call the gRPC test method with "([^"]*)"$`, w.iCall)
	sc.Step(`^the gRPC call should succeed with "([^"]*)"$`, w.callShouldSucceed)
	sc.Step(`^the gRPC call should fail with (\w+) and the message "([^"]*)"$`, w.callShouldFailExactly)
	sc.Step(`^the gRPC echo command should have received "([^"]*)"$`, w.commandShouldHaveReceived)
	sc.Step(`^the gRPC echo command should not have been called$`, w.commandShouldNotHaveBeenCalled)
	sc.Step(`^the gRPC log should hold the error "([^"]*)" of "([^"]*)"$`, w.theLogShouldHoldTheError)
}

func (w *World) commandAnswers(ctx context.Context, text string) error {
	w.echoAnswer, w.echoErr, w.echoPanics = text, nil, false
	return nil
}

func (w *World) commandRejected(ctx context.Context, message string) error {
	w.echoErr = errorkind.New(errorkind.ErrInvalid, message)
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

// commandAnswersWithLogging answers text from inside the command logging
// middleware, as the composition root wraps every handler.
func (w *World) commandAnswersWithLogging(ctx context.Context, text string) error {
	w.echoAnswer, w.echoLogged = text, true
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

func (w *World) commandPanics(ctx context.Context) error {
	w.echoPanics = true
	return nil
}

func (w *World) commandTakes(ctx context.Context, ms int, text string) error {
	w.echoDelay = time.Duration(ms) * time.Millisecond
	w.echoAnswer = text
	return nil
}

func (w *World) iCall(ctx context.Context, text string) error {
	conn, err := w.client()
	if err != nil {
		return err
	}
	w.reply, w.sentID, w.callErr = callEchoWithID(conn, text, "")
	logf("when: Echo(%q) -> %q err=%v", text, w.reply, w.callErr)
	return nil
}

func (w *World) callShouldSucceed(ctx context.Context, want string) error {
	if w.callErr != nil || w.reply != want {
		return fmt.Errorf("call: got %q err=%v, want %q", w.reply, w.callErr, want)
	}
	return nil
}

// codeNamed is the code the scenarios name by its canonical name (INVALID_ARGUMENT).
func codeNamed(name string) (codes.Code, error) {
	var code codes.Code
	err := code.UnmarshalJSON([]byte(`"` + name + `"`))
	return code, err
}

func (w *World) callShouldFailExactly(ctx context.Context, codeName, message string) error {
	want, err := codeNamed(codeName)
	if err != nil {
		return err
	}
	got := status.Convert(w.callErr)
	if w.callErr == nil || got.Code() != want || got.Message() != message {
		return fmt.Errorf("call error: got %v, want %s %q", w.callErr, want, message)
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
