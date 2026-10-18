package test

import (
	"context"
	"errors"
	"fmt"
	"os"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/startup"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

func registerStartupSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the process starts with PlanItems for the titles "([^"]*)"$`, w.theProcessStartsWithPlanItems)
	sc.Step(`^the process starts with a command whose handler fails with "([^"]*)"$`, w.theProcessStartsWithFailingHandler)
	sc.Step(`^the process starts with a command rejected with the kind "([^"]*)"$`, w.theProcessStartsWithKind)
	sc.Step(`^the output should be the JSON line '([^']*)'$`, w.theOutputShouldBe)
	sc.Step(`^the output should be empty$`, w.theOutputShouldBeEmpty)
	sc.Step(`^the exit code should be (\d+)$`, w.theExitCodeShouldBe)
	sc.Step(`^the environment variable "([^"]*)" is "([^"]*)"$`, w.theEnvironmentVariableIs)
	sc.Step(`^the startup input is read from "([^"]*)"$`, w.theStartupInputIsRead)
	sc.Step(`^the startup input should be "([^"]*)"$`, w.theStartupInputShouldBe)
	sc.Step(`^the startup input should be empty$`, w.theStartupInputShouldBeEmpty)
}

func (w *World) theProcessStartsWithPlanItems(ctx context.Context, titles string) error {
	cmd := command.PlanItems{Titles: strings.Split(titles, "|")}
	err := startup.Run(ctx, command.NewPlanItemsHandler(), cmd, &w.out)
	w.exitCode = startup.ExitCode(err)
	logf("when: Run(PlanItems %q) -> err=%v exit=%d", cmd.Titles, err, w.exitCode)
	return nil
}

func (w *World) theProcessStartsWithFailingHandler(ctx context.Context, message string) error {
	failing := pipeline.HandlerFunc[string, string](func(context.Context, string) (string, error) {
		return "", errors.New(message)
	})
	err := startup.Run(ctx, failing, "cmd", &w.out)
	w.exitCode = startup.ExitCode(err)
	logf("when: Run(failing) -> err=%v exit=%d", err, w.exitCode)
	return nil
}

// kinds names every error kind as the scenarios write it.
var kinds = map[string]error{
	"invalid":             pipeline.ErrInvalid,
	"not found":           pipeline.ErrNotFound,
	"conflict":            pipeline.ErrConflict,
	"concurrent change":   pipeline.ErrConcurrentChange,
	"failed precondition": pipeline.ErrFailedPrecondition,
	"unauthenticated":     pipeline.ErrUnauthenticated,
	"permission denied":   pipeline.ErrPermissionDenied,
	"resource exhausted":  pipeline.ErrResourceExhausted,
	"unimplemented":       pipeline.ErrUnimplemented,
	"unavailable":         pipeline.ErrUnavailable,
}

func (w *World) theProcessStartsWithKind(ctx context.Context, name string) error {
	kind, ok := kinds[name]
	if !ok {
		return fmt.Errorf("unknown kind %q", name)
	}
	rejecting := pipeline.HandlerFunc[string, string](func(context.Context, string) (string, error) {
		return "", fmt.Errorf("%w: the reason", kind)
	})
	err := startup.Run(ctx, rejecting, "cmd", &w.out)
	w.exitCode = startup.ExitCode(err)
	logf("when: Run(%s) -> err=%v exit=%d", name, err, w.exitCode)
	return nil
}

func (w *World) theOutputShouldBe(ctx context.Context, want string) error {
	got := w.out.String()
	logf("then: output=%q", got)
	if got != want+"\n" {
		return fmt.Errorf("output: got %q, want %q", got, want+"\n")
	}
	return nil
}

func (w *World) theOutputShouldBeEmpty(ctx context.Context) error {
	if w.out.Len() != 0 {
		return fmt.Errorf("output: got %q, want nothing", w.out.String())
	}
	return nil
}

func (w *World) theExitCodeShouldBe(ctx context.Context, want int) error {
	if w.exitCode != want {
		return fmt.Errorf("exit code: got %d, want %d", w.exitCode, want)
	}
	return nil
}

func (w *World) theEnvironmentVariableIs(ctx context.Context, name, value string) error {
	return os.Setenv(name, value)
}

func (w *World) theStartupInputIsRead(ctx context.Context, name string) error {
	w.input = startup.EnvList(name)
	logf("when: EnvList(%q) -> %q", name, w.input)
	return nil
}

func (w *World) theStartupInputShouldBe(ctx context.Context, want string) error {
	if got := strings.Join(w.input, "|"); got != want {
		return fmt.Errorf("input: got %q, want %q", got, want)
	}
	return nil
}

func (w *World) theStartupInputShouldBeEmpty(ctx context.Context) error {
	if w.input != nil {
		return fmt.Errorf("input: got %q, want nil", w.input)
	}
	return nil
}
