package test

import (
	"bytes"
	"context"
	"errors"
	"fmt"
	"log/slog"
	"strings"
	"sync"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/api/entry"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	entries []entry.Entry

	mu      sync.Mutex
	running map[string]bool // true while an entry's Run has not returned

	err      error
	exitCode int
	log      bytes.Buffer
}

// kinds names every error kind as the scenarios write it.
var kinds = map[string]error{
	"invalid":             pipeline.ErrInvalid,
	"not found":           pipeline.ErrNotFound,
	"conflict":            pipeline.ErrConflict,
	"concurrent change":   pipeline.ErrConcurrentChange,
	"failed precondition": pipeline.ErrFailedPrecondition,
	"unauthenticated":     pipeline.ErrUnauthenticated,
	"permission denied":   pipeline.ErrPermissionDenied,
	"resource exhausted":  pipeline.ErrResourceExhausted,
	"unimplemented":       pipeline.ErrUnimplemented,
	"unavailable":         pipeline.ErrUnavailable,
}

func registerEntrySteps(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{running: map[string]bool{}}
		w.log.Reset()
		return ctx, nil
	})
	sc.Step(`^a long-running entry "([^"]*)"$`, w.aLongRunningEntry)
	sc.Step(`^a one-shot entry "([^"]*)" that succeeds$`, w.aOneShotEntrySucceeds)
	sc.Step(`^an entry "([^"]*)" that fails with "([^"]*)"$`, w.anEntryFails)
	sc.Step(`^a one-shot entry "([^"]*)" that fails with the kind "([^"]*)"$`, w.aOneShotEntryFailsWithKind)
	sc.Step(`^the entry points run together$`, w.theEntryPointsRun)
	sc.Step(`^the entry points run together and the process is told to terminate$`, w.theEntryPointsRunAndTerminate)
	sc.Step(`^every entry point should have stopped$`, w.everyEntryShouldHaveStopped)
	sc.Step(`^running should end without an error$`, w.runningShouldEndWithoutAnError)
	sc.Step(`^running should end with the error "([^"]*)"$`, w.runningShouldEndWithTheError)
	sc.Step(`^the exit code should be (\d+)$`, w.theExitCodeShouldBe)
	sc.Step(`^an entry "([^"]*)" that reports its own error "([^"]*)" of the kind "([^"]*)"$`, w.anEntryReportsItsOwnError)
	sc.Step(`^the log should name the entry "([^"]*)" with its error$`, w.theLogShouldNameTheEntry)
	sc.Step(`^the log should name no entry$`, w.theLogShouldNameNoEntry)
}

// add registers an entry whose Run marks it running until body returns.
func (w *World) add(name string, body func(ctx context.Context) error) {
	w.entries = append(w.entries, entry.Entry{Name: name, Run: func(ctx context.Context) error {
		w.mu.Lock()
		w.running[name] = true
		w.mu.Unlock()
		defer func() {
			w.mu.Lock()
			w.running[name] = false
			w.mu.Unlock()
		}()
		return body(ctx)
	}})
}

// waitOthers gives the long-running entries time to start before a one-shot
// or failing entry ends, so the scenario proves they are stopped, not skipped.
func waitOthers() { time.Sleep(20 * time.Millisecond) }

func (w *World) aLongRunningEntry(ctx context.Context, name string) error {
	w.add(name, func(ctx context.Context) error {
		<-ctx.Done()
		return nil
	})
	return nil
}

func (w *World) aOneShotEntrySucceeds(ctx context.Context, name string) error {
	w.add(name, func(context.Context) error {
		waitOthers()
		return nil
	})
	return nil
}

func (w *World) anEntryFails(ctx context.Context, name, message string) error {
	w.add(name, func(context.Context) error {
		waitOthers()
		return errors.New(message)
	})
	return nil
}

func (w *World) aOneShotEntryFailsWithKind(ctx context.Context, name, kindName string) error {
	kind, ok := kinds[kindName]
	if !ok {
		return fmt.Errorf("unknown kind %q", kindName)
	}
	w.add(name, func(context.Context) error { return fmt.Errorf("%w: the reason", kind) })
	return nil
}

// run calls RunAll and fails the step if it does not return in time.
func (w *World) run(ctx context.Context) error {
	done := make(chan error, 1)
	logger := slog.New(slog.NewTextHandler(&w.log, nil))
	go func() { done <- entry.RunAll(ctx, logger, w.entries...) }()
	select {
	case w.err = <-done:
		w.exitCode = entry.ExitCode(w.err)
		logf("when: RunAll -> err=%v exit=%d", w.err, w.exitCode)
		return nil
	case <-time.After(5 * time.Second):
		return fmt.Errorf("RunAll did not return")
	}
}

func (w *World) theEntryPointsRun(ctx context.Context) error {
	return w.run(context.Background())
}

func (w *World) theEntryPointsRunAndTerminate(ctx context.Context) error {
	sigterm, cancel := context.WithCancel(context.Background())
	time.AfterFunc(20*time.Millisecond, cancel)
	return w.run(sigterm)
}

func (w *World) everyEntryShouldHaveStopped(ctx context.Context) error {
	w.mu.Lock()
	defer w.mu.Unlock()
	if len(w.running) != len(w.entries) {
		return fmt.Errorf("started %d of %d entries", len(w.running), len(w.entries))
	}
	for name, running := range w.running {
		if running {
			return fmt.Errorf("entry %q is still running", name)
		}
	}
	return nil
}

func (w *World) runningShouldEndWithoutAnError(ctx context.Context) error {
	if w.err != nil {
		return fmt.Errorf("RunAll returned %v, want nil", w.err)
	}
	return nil
}

func (w *World) runningShouldEndWithTheError(ctx context.Context, message string) error {
	if w.err == nil || w.err.Error() != message {
		return fmt.Errorf("RunAll returned %v, want %q", w.err, message)
	}
	return nil
}

func (w *World) theExitCodeShouldBe(ctx context.Context, want int) error {
	if w.exitCode != want {
		return fmt.Errorf("exit code: got %d, want %d", w.exitCode, want)
	}
	return nil
}

func (w *World) anEntryReportsItsOwnError(ctx context.Context, name, message, kindName string) error {
	w.add(name, func(context.Context) error { return fmt.Errorf("%w: %s", kinds[kindName], message) })
	w.entries[len(w.entries)-1].ReportsErrors = true
	return nil
}

func (w *World) theLogShouldNameTheEntry(ctx context.Context, name string) error {
	if !strings.Contains(w.log.String(), "entry="+name) || !strings.Contains(w.log.String(), "error=") {
		return fmt.Errorf("log %q names no entry %q with its error", w.log.String(), name)
	}
	return nil
}

func (w *World) theLogShouldNameNoEntry(ctx context.Context) error {
	if strings.Contains(w.log.String(), "entry=") {
		return fmt.Errorf("log %q names an entry", w.log.String())
	}
	return nil
}
