// Source: skill vp-cli-api-in-go

// Package cli is the command-line entry point: `{app} {resource} {action}
// [flags]` runs one command and exits. It turns the arguments into a command,
// calls its handler, and prints the result - one JSON line by default, or text
// with --output text. Like every entry point it knows only the command package
// - never domain, service, or infrastructure. Other packages import it as
// cliapi. It is built on cobra (github.com/spf13/cobra), which gives the help
// of every level, flag parsing, --version, and shell completion.
package cli

import (
	"bytes"
	"context"
	"errors"
	"fmt"
	"io"
	"log/slog"
	"time"

	"github.com/spf13/cobra"
	"github.com/spf13/pflag"

	"{module-path}/internal/command/pipeline"
	"{module-path}/internal/logging"
)

// Resource is one resource of the command line - `todo items ...` - with its
// actions, built by Command in the resource's file of the endpoint package.
type Resource struct {
	// Name is the resource as typed: "items".
	Name string
	// Short is its one-line help.
	Short string
	// Actions are its commands: `todo items plan`.
	Actions []*cobra.Command
}

// Run is the CLI entry point's one call from the composition root: it builds
// the command line `name` from the resources, runs the action args name, and
// returns its error. An error of the action's handler is returned as it is,
// so the process exit code follows its kind; an error of the command line
// itself - an unknown action, a bad flag - wraps pipeline.ErrInvalid (exit
// code 2). Help goes to stdout, every error to stderr. One "cli call" record
// is logged per run (see logCall); the run gets a request id of its own, so
// every record of it carries the same one once the logging variation point is
// attached.
func Run(ctx context.Context, logger *slog.Logger, name, version string, args []string, resources []Resource, stdout, stderr io.Writer) error {
	ctx = logging.WithRequestID(ctx, "")
	start := time.Now()
	root := &cobra.Command{
		Use:           name,
		Args:          noArgs,
		RunE:          showHelp,
		Short:         name + " - the command line of the application",
		Version:       version,
		SilenceErrors: true, // Run prints the error once, below
		SilenceUsage:  true, // a failed command is not a usage error
	}
	root.PersistentFlags().String(outputFlag, outputJSON, "output format: json or text")
	root.SetFlagErrorFunc(func(cmd *cobra.Command, err error) error {
		return fmt.Errorf("%w: %s (see %s --help)", pipeline.ErrInvalid, err, cmd.CommandPath())
	})
	for _, resource := range resources {
		group := &cobra.Command{Use: resource.Name, Short: resource.Short, Args: noArgs, RunE: showHelp}
		group.AddCommand(resource.Actions...)
		root.AddCommand(group)
	}
	root.SetArgs(args)
	var output bytes.Buffer // what the action printed, for the record of the run
	root.SetOut(io.MultiWriter(stdout, &output))
	root.SetErr(stderr)

	ran, err := root.ExecuteContextC(ctx)
	defer func() { logCall(ctx, logger, ran, start, err, output.String()) }()
	var handled handlerError
	switch {
	case err == nil:
		return nil
	case errors.As(err, &handled):
		err = handled.err
	case pipeline.KindOf(err) == nil:
		// cobra's own errors - an unknown command, a missing argument - carry no kind
		err = fmt.Errorf("%w: %s (see %s --help)", pipeline.ErrInvalid, err, name)
	}
	fmt.Fprintln(stderr, "Error:", err)
	return err
}

// logCall writes the record of one run - "cli call" with the action run, the
// duration, failed, and as the payload the flags given on the command line and
// the output printed, as printed.
func logCall(ctx context.Context, logger *slog.Logger, ran *cobra.Command, start time.Time, err error, output string) {
	flags := map[string]string{}
	ran.Flags().Visit(func(f *pflag.Flag) { flags[f.Name] = f.Value.String() })
	logger.LogAttrs(ctx, slog.LevelInfo, "cli call",
		slog.String("action", ran.CommandPath()), slog.Duration("duration", time.Since(start)),
		logging.Failed(err != nil), logging.Payload(map[string]any{"flags": flags, "output": output}))
}

// noArgs rejects an argument where an action is expected, so `todo items
// bogus` is an unknown command instead of the group's help.
func noArgs(cmd *cobra.Command, args []string) error {
	if len(args) > 0 {
		return fmt.Errorf("%w: unknown command %q for %q (see %s --help)", pipeline.ErrInvalid, args[0], cmd.CommandPath(), cmd.CommandPath())
	}
	return nil
}

// showHelp answers a resource, or the application, called with no action:
// its help, as `--help` would.
func showHelp(cmd *cobra.Command, _ []string) error {
	return cmd.Help()
}

// handlerError marks an error returned by an action's handler, so Run tells
// it from an error of the command line.
type handlerError struct{ err error }

func (e handlerError) Error() string { return e.err.Error() }
func (e handlerError) Unwrap() error { return e.err }
