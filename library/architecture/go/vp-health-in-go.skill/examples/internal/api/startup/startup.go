// Source: skill base-plateau-in-go

// Package startup is the base plateau's only entry point: the process runs one
// command when it starts. Like every entry point, it knows the command
// package (the command types) and the pipeline package (the Handler contract
// and the error kind), and nothing below them.
//
// It exists only in the base plateau. When an API variation point is attached
// (HTTP, gRPC, CLI, a listener), its Entry replaces the startup Entry in the
// "// [entries]" section of cmd/{app}/main.go and this package is deleted.
// The exit code of the process is package entry's, which stays.
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

	"github.com/example/todo/internal/command/pipeline"
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
