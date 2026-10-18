// Source: skill base-plateau-in-go

// Package startup is the base plateau's only entry point: the process runs one
// command when it starts. Like every entry point, it knows the command
// package (the command types) and the pipeline package (the Handler contract
// and the error kind), and nothing below them.
//
// It exists only in the base plateau. When an API variation point is attached
// (HTTP, gRPC, CLI, a listener), its entry replaces the startup lines in the
// "// [entries]" section of cmd/{app}/main.go and this package is deleted.
// It stays only in an application whose whole job is one command per run
// (a job started by a scheduler), or, rarely, beside a server that must run
// one command once at start.
package startup

import (
	"context"
	"encoding/json"
	"io"
	"os"
	"strings"

	"{module-path}/internal/command/pipeline"
)

// Exit codes of a process whose work is the startup command. A script or a
// scheduler that started the process reads them to tell the outcomes apart.
const (
	ExitOK       = 0 // the command succeeded; its result is on standard output
	ExitFailure  = 1 // the service failed, or a dependency is unavailable
	ExitRejected = 2 // the command was rejected: its error has a kind other than unavailable
)

// EnvList reads the comma-separated environment variable name as a list -
// the way the startup command's input is given, since the process has no other
// input. Unset or empty is nil. The startup entry owns this setting, so it
// disappears together with the entry.
func EnvList(name string) []string {
	raw := os.Getenv(name)
	if raw == "" {
		return nil
	}
	return strings.Split(raw, ",")
}

// Run executes cmd once through handler and writes the result to out as one
// JSON line. It works with any command: TCommand and TResult are taken from
// the handler it is given. On error nothing is written and the error is returned.
func Run[TCommand, TResult any](ctx context.Context, handler pipeline.Handler[TCommand, TResult], cmd TCommand, out io.Writer) error {
	res, err := handler.Handle(ctx, cmd)
	if err != nil {
		return err
	}
	return json.NewEncoder(out).Encode(res)
}

// ExitCode maps the error Run returned to the process exit code by its kind
// (skill error-kinds): nil is ExitOK; an error the request caused - any kind
// but unavailable - is ExitRejected; unavailable, and an error with no kind,
// are ExitFailure, the outcomes a retry of the same request may change.
func ExitCode(err error) int {
	switch kind := pipeline.KindOf(err); {
	case err == nil:
		return ExitOK
	case kind == nil, kind == pipeline.ErrUnavailable:
		return ExitFailure
	default:
		return ExitRejected
	}
}
