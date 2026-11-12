package test

import (
	"fmt"
	"strings"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/command"
)

func registerItemsSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^PlanItems returns the items "([^"]*)"$`, func(titles string) error {
		for _, title := range strings.Split(titles, "|") {
			w.planResult.Items = append(w.planResult.Items, command.ItemView{Title: title})
		}
		return nil
	})
	sc.Step(`^I run "([^"]*)"$`, w.run)
	sc.Step(`^stdout should be the line '([^']*)'$`, func(want string) error {
		if got := w.stdout.String(); got != want+"\n" {
			return fmt.Errorf("stdout: got %q, want %q", got, want+"\n")
		}
		return nil
	})
	sc.Step(`^stdout should be the lines "([^"]*)"$`, func(lines string) error {
		want := strings.ReplaceAll(lines, "|", "\n") + "\n"
		if got := w.stdout.String(); got != want {
			return fmt.Errorf("stdout: got %q, want %q", got, want)
		}
		return nil
	})
	sc.Step(`^PlanItems should have received the titles "([^"]*)"$`, func(titles string) error {
		if got := strings.Join(w.planGot.Titles, "|"); got != titles {
			return fmt.Errorf("PlanItems received %q, want %q", got, titles)
		}
		return nil
	})
}
