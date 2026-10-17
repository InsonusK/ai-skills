package test

import (
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/startup"
	"github.com/example/todo/internal/application"
)

func registerStartupSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the process starts with PlanItems for the titles "([^"]*)"$`, w.theProcessStartsWithPlanItems)
	sc.Step(`^the process starts with a command whose handler fails with "([^"]*)"$`, w.theProcessStartsWithFailingHandler)
	sc.Step(`^the output should be the JSON line '([^']*)'$`, w.theOutputShouldBe)
	sc.Step(`^the output should be empty$`, w.theOutputShouldBeEmpty)
	sc.Step(`^the exit code should be (\d+)$`, w.theExitCodeShouldBe)
}

func (w *World) theProcessStartsWithPlanItems(ctx context.Context, titles string) error {
	cmd := application.PlanItems{Titles: strings.Split(titles, "|")}
	err := startup.Run(ctx, application.NewPlanItemsHandler(), cmd, &w.out)
	w.exitCode = startup.ExitCode(err)
	logf("when: Run(PlanItems %q) -> err=%v exit=%d", cmd.Titles, err, w.exitCode)
	return nil
}

func (w *World) theProcessStartsWithFailingHandler(ctx context.Context, message string) error {
	failing := application.HandlerFunc[string, string](func(context.Context, string) (string, error) {
		return "", errors.New(message)
	})
	err := startup.Run(ctx, failing, "cmd", &w.out)
	w.exitCode = startup.ExitCode(err)
	logf("when: Run(failing) -> err=%v exit=%d", err, w.exitCode)
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
