// Source: skill base-plateau-in-go

// Package startup is the base plateau's only entry point: the process runs one
// command when it starts. Like every entry point, it knows the application
// package (commands and the Handler contract) and nothing below it.
package startup

import (
	"context"
	"encoding/json"
	"errors"
	"io"

	"github.com/example/todo/internal/application"
)

// Exit codes of a process whose work is the startup command.
const (
	ExitOK       = 0
	ExitFailure  = 1
	ExitRejected = 2
)

// Run executes cmd once through h and writes the result to out as one JSON line.
func Run[C, R any](ctx context.Context, h application.Handler[C, R], cmd C, out io.Writer) error {
	res, err := h.Handle(ctx, cmd)
	if err != nil {
		return err
	}
	return json.NewEncoder(out).Encode(res)
}

// ExitCode maps the error Run returned to the process exit code: a command
// rejected by a business rule is ExitRejected, any other error ExitFailure.
func ExitCode(err error) int {
	switch {
	case err == nil:
		return ExitOK
	case errors.Is(err, application.ErrInvalid):
		return ExitRejected
	default:
		return ExitFailure
	}
}
