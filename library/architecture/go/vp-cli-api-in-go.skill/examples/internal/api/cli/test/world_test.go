package test

import (
	"bytes"
	"context"
	"errors"
	"fmt"
	"strings"

	"github.com/cucumber/godog"
	"github.com/spf13/cobra"
	"github.com/spf13/pflag"

	cliapi "github.com/example/todo/internal/api/cli"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	// the fake echo handler - a command of these tests only, so the generic
	// entry point is tested without any command of the application
	echoAnswer string
	echoErr    error
	echoCalled bool
	echoGot    echoCommand

	stdout, stderr bytes.Buffer
	err            error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
		return ctx, nil
	})
}

// echoCommand and echoResult stand for any command and its result.
type echoCommand struct{ Text string }
type echoResult struct{ Text string }

func (w *World) echoHandler() pipeline.Handler[echoCommand, echoResult] {
	return pipeline.HandlerFunc[echoCommand, echoResult](
		func(_ context.Context, cmd echoCommand) (echoResult, error) {
			w.echoCalled, w.echoGot = true, cmd
			return echoResult{Text: w.echoAnswer}, w.echoErr
		})
}

// resources is the one test resource `test` with two actions: echo has a text
// form, plain has none.
func (w *World) resources() []cliapi.Resource {
	bind := func(flags *pflag.FlagSet) func() (echoCommand, error) {
		text := flags.String("text", "", "the text to send")
		return func() (echoCommand, error) {
			if *text == "" {
				return echoCommand{}, errors.New("--text is required")
			}
			return echoCommand{Text: *text}, nil
		}
	}
	type output struct {
		Text string `json:"text"`
	}
	encode := func(result echoResult) any { return output{Text: result.Text} }
	text := func(result echoResult) string { return "echo: " + result.Text }
	return []cliapi.Resource{{Name: "test", Short: "Test actions", Actions: []*cobra.Command{
		cliapi.Command("echo", "Echo a text", w.echoHandler(), bind, encode, text),
		cliapi.Command("plain", "Echo a text, with no text form", w.echoHandler(), bind, encode, nil),
	}}}
}

// run runs the command line with args split on spaces.
func (w *World) run(ctx context.Context, line string) {
	args := strings.Fields(line)
	w.err = cliapi.Run(ctx, "todo", "1.2.3", args, w.resources(), &w.stdout, &w.stderr)
	logf("when: todo %s -> err=%v\nstdout: %s\nstderr: %s", line, w.err, w.stdout.String(), w.stderr.String())
}
