// Source: skill base-plateau-in-go

// Package startup is the base plateau's only entry point: the process runs one
// command when it starts. Like every entry point, it knows the command
// package (commands and the Handler contract) and nothing below it.
package startup

import (
	"context"
	"encoding/json"
	"errors"
	"io"

	"github.com/example/todo/internal/command"
)

// Exit codes of a process whose work is the startup command. A script or a
// scheduler that started the process reads them to tell the outcomes apart.
const (
	ExitOK       = 0 // the command succeeded; its result is on standard output
	ExitFailure  = 1 // the command failed for a reason other than its input
	ExitRejected = 2 // the command broke a business rule (command.ErrInvalid)
)

// Run executes cmd once through handler and writes the result to out as one
// JSON line. It works with any command: TCommand and TResult are taken from
// the handler it is given. On error nothing is written and the error is returned.
func Run[TCommand, TResult any](ctx context.Context, handler command.Handler[TCommand, TResult], cmd TCommand, out io.Writer) error {
	res, err := handler.Handle(ctx, cmd)
	if err != nil {
		return err
	}
	return json.NewEncoder(out).Encode(res)
}

// ExitCode maps the error Run returned to the process exit code: nil is
// ExitOK, a command rejected by a business rule is ExitRejected, any other
// error ExitFailure.
func ExitCode(err error) int {
	switch {
	case err == nil:
		return ExitOK
	case errors.Is(err, command.ErrInvalid):
		return ExitRejected
	default:
		return ExitFailure
	}
}
