// Source: skill vp-cli-api-in-go

package pipeline

import (
	"encoding/json"
	"fmt"
	"io"

	"github.com/spf13/cobra"
	"github.com/spf13/pflag"

	commandpipeline "github.com/example/todo/internal/command/pipeline"
)

// The values of the --output flag.
const (
	outputFlag = "output"
	outputJSON = "json"
	outputText = "text"
)

// Command turns one command handler into one action of the command line. It
// is written once for every command, the same way http.Endpoint is:
//
//  1. bind declares the action's flags on flags and returns read, which turns
//     the parsed flags into the command (TCommand); an error of read is a
//     usage error (exit code 2) and the handler is not called;
//  2. the handler runs the command;
//  3. an error is returned to Run, which prints it and keeps its kind;
//  4. encode turns the result (TResult) into the output written as one JSON
//     line, or, with --output text, text writes it for a person - when text
//     is nil, --output text writes indented JSON.
//
// bind, encode, and text belong to the action (its resource's file in the
// endpoint package, e.g. endpoint/items.go): the flags and the output are the
// command line's contract, owned by this package, never the command's types.
func Command[TCommand, TResult any](
	use, short string,
	handler commandpipeline.Handler[TCommand, TResult],
	bind func(flags *pflag.FlagSet) (read func() (TCommand, error)),
	encode func(result TResult) any,
	text func(result TResult) string,
) *cobra.Command {
	action := &cobra.Command{Use: use, Short: short, Args: noArgs}
	read := bind(action.Flags())
	action.RunE = func(cmd *cobra.Command, _ []string) error {
		output, err := cmd.Flags().GetString(outputFlag)
		if err != nil || (output != outputJSON && output != outputText) {
			return fmt.Errorf("%w: --output must be json or text, not %q", commandpipeline.ErrInvalid, output)
		}
		c, err := read()
		if err != nil {
			return fmt.Errorf("%w: %s (see %s --help)", commandpipeline.ErrInvalid, err, cmd.CommandPath())
		}
		result, err := handler.Handle(cmd.Context(), c)
		if err != nil {
			return handlerError{err}
		}
		return write(cmd.OutOrStdout(), output, encode(result), func() string {
			if text == nil {
				return ""
			}
			return text(result)
		})
	}
	return action
}

// write prints the result in the chosen output format.
func write(out io.Writer, output string, encoded any, text func() string) error {
	if output == outputText {
		if t := text(); t != "" {
			_, err := fmt.Fprintln(out, t)
			return err
		}
		encoder := json.NewEncoder(out)
		encoder.SetIndent("", "  ")
		return encoder.Encode(encoded)
	}
	return json.NewEncoder(out).Encode(encoded)
}
