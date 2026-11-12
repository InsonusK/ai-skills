// Source: skill base-plateau-in-go

// Package entry runs the application's entry points - the startup command, an
// HTTP server, a gRPC server, the health endpoints - together in one process,
// and turns how they ended into the process exit code. It is what lets every
// variation point attach by adding or replacing one Entry in the
// "// [entries]" section of cmd/{app}/main.go: no variation point writes code
// to run itself beside another.
package entry

import (
	"context"
	"log/slog"

	"{module-path}/internal/command/pipeline"
)

// Entry is one entry point of the application.
type Entry struct {
	// Name is how logs name the entry point, e.g. "http".
	Name string
	// Run serves until ctx is cancelled and then returns nil, or returns
	// early with the error that stopped it. A one-shot entry (the startup
	// command) returns when its work is done.
	Run func(ctx context.Context) error
	// ReportsErrors is true for an entry that shows its own error to whoever
	// called it - the CLI prints "Error: ..." on stderr - so RunAll does not
	// log that error a second time. The error still sets the exit code.
	ReportsErrors bool
}

// RunAll runs every entry at once and waits until the first one ends - its
// work done, its error, or ctx cancelled on SIGTERM. Then it cancels the
// others, waits for them to stop, and returns the first error any entry
// returned, or nil. So one failed entry stops the whole process instead of
// leaving it half-alive, and a one-shot entry ends the process when it is done.
func RunAll(ctx context.Context, logger *slog.Logger, entries ...Entry) error {
	ctx, cancel := context.WithCancel(ctx)
	defer cancel()

	type ended struct {
		entry Entry
		err   error
	}
	results := make(chan ended, len(entries))
	for _, e := range entries {
		go func() { results <- ended{e, e.Run(ctx)} }()
	}

	var first error
	for i := range entries {
		r := <-results
		if r.err != nil {
			if !r.entry.ReportsErrors {
				logger.Error("entry point stopped with an error", "entry", r.entry.Name, "error", r.err)
			}
			if first == nil {
				first = r.err
			}
		}
		if i == 0 {
			cancel() // the first entry ended: stop the others
		}
	}
	return first
}

// Exit codes of the process. A script, a scheduler, or an orchestrator reads
// them to tell the outcomes apart.
const (
	ExitOK       = 0 // every entry ended without an error
	ExitFailure  = 1 // the service failed, or a dependency is unavailable: a retry may help
	ExitRejected = 2 // the request was rejected: its error has a kind other than unavailable
)

// ExitCode maps the error RunAll returned to the exit code by its kind
// (skill error-kinds): nil is ExitOK; unavailable, and an error with no kind -
// a server that could not start - are ExitFailure; any other kind, which only
// a one-shot command returns, is ExitRejected.
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
