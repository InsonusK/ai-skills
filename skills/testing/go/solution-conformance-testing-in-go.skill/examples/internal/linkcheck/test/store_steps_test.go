package test

import (
	"context"
	"errors"
	"fmt"
	"github.com/cucumber/godog"
	"github.com/example/linkcheck/internal/linkcheck"
	"os"
	"path/filepath"
	"sync"
	"testing"
)

func registerStoreSteps(sc *godog.ScenarioContext, w *World, t *testing.T) {
	newStore := func() string {
		path := filepath.Join(t.TempDir(), "history.jsonl")
		w.store = linkcheck.NewHistoryStore(path)
		return path
	}
	sc.Step(`^an empty history file$`, func(ctx context.Context) error {
		path := newStore()
		logf("empty history=%s", path)
		return os.WriteFile(path, nil, 0600)
	})
	sc.Step(`^no history file$`, func(ctx context.Context) error { logf("missing history=%s", newStore()); return nil })
	sc.Step(`^a history file with the line "([^"]*)"$`, func(ctx context.Context, line string) error {
		path := newStore()
		logf("history line=%q", line)
		return os.WriteFile(path, []byte(line+"\n"), 0600)
	})
	sc.Step(`^I store the check of "([^"]*)"$`, func(ctx context.Context, input string) error {
		result := linkcheck.Check(input)
		logf("append=%+v", result)
		return w.store.Append(result)
	})
	sc.Step(`^the history holds (\d+) checks?$`, func(ctx context.Context, want int) error {
		var err error
		w.history, err = w.store.Load()
		logf("history count=%d want=%d error=%v", len(w.history), want, err)
		if err != nil {
			return err
		}
		if len(w.history) != want {
			return fmt.Errorf("history got %d want %d", len(w.history), want)
		}
		return nil
	})
	sc.Step(`^the last stored URL is "([^"]*)"$`, func(ctx context.Context, want string) error {
		if len(w.history) == 0 {
			return fmt.Errorf("history empty")
		}
		got := w.history[len(w.history)-1].Normalized
		logf("last=%q want=%q", got, want)
		if got != want {
			return fmt.Errorf("last URL got %q want %q", got, want)
		}
		return nil
	})
	sc.Step(`^I read the history$`, func(ctx context.Context) error {
		w.history, w.storeErr = w.store.Load()
		logf("load error=%v", w.storeErr)
		return nil
	})
	sc.Step(`^reading fails because the store is corrupted$`, func(ctx context.Context) error {
		logf("load error=%v", w.storeErr)
		if !errors.Is(w.storeErr, linkcheck.ErrStoreCorrupted) {
			return fmt.Errorf("got %v want corruption", w.storeErr)
		}
		return nil
	})
	sc.Step(`^(\d+) writers store (\d+) checks each at the same time$`, func(ctx context.Context, writers, count int) error {
		start := make(chan struct{})
		failures := make(chan error, writers)
		var wg sync.WaitGroup
		for i := 0; i < writers; i++ {
			wg.Add(1)
			go func() {
				defer wg.Done()
				<-start
				for j := 0; j < count; j++ {
					if err := w.store.Append(linkcheck.Check("https://a.example")); err != nil {
						failures <- err
						return
					}
				}
			}()
		}
		close(start)
		wg.Wait()
		close(failures)
		logf("writers=%d count=%d completed", writers, count)
		for err := range failures {
			return err
		}
		return nil
	})
}
