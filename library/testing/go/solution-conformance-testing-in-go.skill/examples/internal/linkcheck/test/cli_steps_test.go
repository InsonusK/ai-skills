package test

import (
	"context"
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/linkcheck/internal/linkcheck"
	"slices"
	"strings"
)

func registerCliSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^I run the command with "([^"]*)"$`, func(ctx context.Context, args string) error {
		w.exitCode = linkcheck.Run(strings.Fields(args), &w.out, &w.errOut)
		logf("command exit=%d stdout=%q stderr=%q", w.exitCode, w.out.String(), w.errOut.String())
		return nil
	})
	sc.Step(`^the exit code is (\d+)$`, func(ctx context.Context, want int) error {
		logf("exit=%d want=%d", w.exitCode, want)
		if w.exitCode != want {
			return fmt.Errorf("exit got %d want %d", w.exitCode, want)
		}
		return nil
	})
	sc.Step(`^the output is:$`, func(ctx context.Context, table *godog.Table) error {
		want := []string{}
		for _, r := range table.Rows[1:] {
			want = append(want, r.Cells[0].Value)
		}
		got := strings.Split(strings.TrimSuffix(w.out.String(), "\n"), "\n")
		logf("output=%q want=%q", got, want)
		if !slices.Equal(got, want) {
			return fmt.Errorf("output got %q want %q", got, want)
		}
		return nil
	})
	sc.Step(`^the error output starts with "([^"]*)"$`, func(ctx context.Context, want string) error {
		logf("stderr=%q prefix=%q", w.errOut.String(), want)
		if !strings.HasPrefix(w.errOut.String(), want) {
			return fmt.Errorf("stderr has no %q prefix", want)
		}
		return nil
	})

}
