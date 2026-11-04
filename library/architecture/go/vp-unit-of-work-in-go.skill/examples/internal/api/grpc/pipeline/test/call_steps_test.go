package test

import (
	"context"
	"errors"
	"fmt"
	"strings"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"github.com/example/todo/internal/command/pipeline"
)

func registerCallSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the command answers "([^"]*)"$`, w.commandAnswers)
	sc.Step(`^the command is rejected with "([^"]*)"$`, w.commandRejected)
	sc.Step(`^the command fails with "([^"]*)"$`, w.commandFails)
	sc.Step(`^the command fails with the kind conflict and "([^"]*)"$`, w.commandConflicts)
	sc.Step(`^the command panics$`, w.commandPanics)
	sc.Step(`^the command takes (\d+) milliseconds and answers "([^"]*)"$`, w.commandTakes)
	sc.Step(`^I call the test method with "([^"]*)"$`, w.iCall)
	sc.Step(`^the call should succeed with "([^"]*)"$`, w.callShouldSucceed)
	sc.Step(`^the call should fail with (\w+) and a message containing "([^"]*)"$`, w.callShouldFailContaining)
	sc.Step(`^the call should fail with (\w+) and the message "([^"]*)"$`, w.callShouldFailExactly)
	sc.Step(`^the command should have received "([^"]*)"$`, w.commandShouldHaveReceived)
	sc.Step(`^the command should not have been called$`, w.commandShouldNotHaveBeenCalled)
}

func (w *World) commandAnswers(ctx context.Context, text string) error {
	w.echoAnswer, w.echoErr, w.echoPanics = text, nil, false
	return nil
}

func (w *World) commandRejected(ctx context.Context, message string) error {
	w.echoErr = fmt.Errorf("%w: %s", pipeline.ErrInvalid, message)
	return nil
}

func (w *World) commandFails(ctx context.Context, message string) error {
	w.echoErr = errors.New(message)
	return nil
}

func (w *World) commandConflicts(ctx context.Context, message string) error {
	w.echoErr = fmt.Errorf("%w: %s", pipeline.ErrConflict, message)
	return nil
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
	w.reply, w.callErr = callEcho(conn, text)
	logf("when: Echo(%q) -> %q err=%v", text, w.reply, w.callErr)
	return nil
}

func (w *World) callShouldSucceed(ctx context.Context, want string) error {
	if w.callErr != nil || w.reply != want {
		return fmt.Errorf("call: got %q err=%v, want %q", w.reply, w.callErr, want)
	}
	return nil
}

// codeNamed reads a code by its Go name (InvalidArgument, Internal, ...).
func codeNamed(name string) (codes.Code, error) {
	var code codes.Code
	err := code.UnmarshalJSON([]byte(`"` + strings.ToUpper(snake(name)) + `"`))
	return code, err
}

// snake turns InvalidArgument into INVALID_ARGUMENT's lower form invalid_argument.
func snake(name string) string {
	var b strings.Builder
	for i, r := range name {
		if i > 0 && r >= 'A' && r <= 'Z' {
			b.WriteByte('_')
		}
		b.WriteRune(r)
	}
	return b.String()
}

func (w *World) callShouldFailContaining(ctx context.Context, codeName, text string) error {
	want, err := codeNamed(codeName)
	if err != nil {
		return err
	}
	got := status.Convert(w.callErr)
	if w.callErr == nil || got.Code() != want || !strings.Contains(got.Message(), text) {
		return fmt.Errorf("call error: got %v, want %s with %q", w.callErr, want, text)
	}
	return nil
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
