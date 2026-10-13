package test

import (
	"context"
	"errors"
	"fmt"
	"math/rand/v2"
	"sort"
	"strconv"
	"strings"
	"sync"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/linkcheck-service/internal/taskbox"
)

// --- setup: settings, handlers, enqueue -----------------------------------

func registerSetupSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the TaskBox settings:$`, w.theSettings)
	sc.Step(`^the TaskBox setting "([^"]*)" is (\S+)$`, w.theSetting)
	sc.Step(`^the handler for "([^"]*)" answers:$`, w.theHandlerAnswers)
	sc.Step(`^no handler is registered for "([^"]*)"$`, w.noHandler)
	sc.Step(`^these tasks are enqueued in a transaction that (commits|rolls back):$`, w.enqueueTable)
	sc.Step(`^(\d+) tasks of type "([^"]*)" are enqueued$`, w.enqueueMany)
	sc.Step(`^(\d+) tasks of type "([^"]*)" in group "([^"]*)" are enqueued concurrently, each transaction held open up to (\S+) after its insert$`, w.enqueueConcurrently)
}

func (w *World) theSettings(ctx context.Context, table *godog.Table) error {
	for _, row := range table.Rows[1:] {
		if err := w.theSetting(ctx, row.Cells[0].Value, row.Cells[1].Value); err != nil {
			return err
		}
	}
	return nil
}

func (w *World) theSetting(ctx context.Context, name, value string) error {
	var err error
	s := &w.settings
	switch name {
	case "lease":
		s.lease, err = time.ParseDuration(value)
	case "backoff base":
		s.backoffBase, err = time.ParseDuration(value)
	case "backoff cap":
		s.backoffCap, err = time.ParseDuration(value)
	case "default retention":
		s.retention, err = time.ParseDuration(value)
	case "poll interval":
		s.poll, err = time.ParseDuration(value)
	case "partitions":
		s.partitions, err = strconv.Atoi(value)
	default:
		return fmt.Errorf("unknown TaskBox setting %q", name)
	}
	if err != nil {
		return fmt.Errorf("setting %q: %w", name, err)
	}
	logf("given: setting %s = %s", name, value)
	return w.sut.configure(w.settings)
}

func (w *World) theHandlerAnswers(ctx context.Context, taskType string, table *godog.Table) error {
	var rules []handlerRule
	for _, row := range rowsAsMaps(table) {
		var r handlerRule
		var err error
		if row["attempt"] != "*" {
			if r.attempt, err = strconv.Atoi(row["attempt"]); err != nil {
				return err
			}
		}
		if v := row["status"]; v != "" {
			if r.status, err = strconv.Atoi(v); err != nil {
				return err
			}
		}
		if r.retryAfter, err = optDuration(row["retry after"]); err != nil {
			return err
		}
		if r.delay, err = optDuration(row["delay"]); err != nil {
			return err
		}
		r.err = row["error"]
		rules = append(rules, r)
	}
	w.registry.Register(taskType, w.scripted(rules))
	logf("given: handler %q scripted with %d rule(s)", taskType, len(rules))
	return nil
}

func (w *World) noHandler(ctx context.Context, taskType string) error {
	w.registry.Unregister(taskType)
	logf("given: no handler for %q", taskType)
	return nil
}

func (w *World) enqueueTable(ctx context.Context, outcome string, table *godog.Table) error {
	var tasks []taskbox.NewTask
	for _, row := range rowsAsMaps(table) {
		t := taskbox.NewTask{Type: row["type"], Group: row["group"], IdempotencyKey: row["idempotency key"]}
		if v := row["run at"]; v != "" {
			d, err := time.ParseDuration(strings.TrimPrefix(v, "+"))
			if err != nil {
				return err
			}
			t.RunAt = time.Now().Add(d)
		}
		if v := row["max attempts"]; v != "" {
			n, err := strconv.Atoi(v)
			if err != nil {
				return err
			}
			t.MaxAttempts = n
		}
		var err error
		if t.Retention, err = optDuration(row["retention"]); err != nil {
			return err
		}
		tasks = append(tasks, w.remember(row["task"], t))
	}
	added, err := w.sut.enqueue(ctx, outcome == "commits", tasks)
	logf("when: enqueued %d task(s), transaction %s -> added=%v err=%v", len(tasks), outcome, added, err)
	return err
}

func (w *World) enqueueMany(ctx context.Context, n int, taskType string) error {
	for i := 1; i <= n; i++ {
		t := w.remember(fmt.Sprintf("task-%d", i), taskbox.NewTask{Type: taskType})
		if _, err := w.sut.enqueue(ctx, true, []taskbox.NewTask{t}); err != nil {
			return err
		}
	}
	logf("when: enqueued %d ungrouped %q task(s)", n, taskType)
	return nil
}

func (w *World) enqueueConcurrently(ctx context.Context, n int, taskType, group, holdText string) error {
	hold, err := time.ParseDuration(holdText)
	if err != nil {
		return err
	}
	tasks := make([]taskbox.NewTask, n)
	for i := range tasks {
		tasks[i] = w.remember(fmt.Sprintf("%s-%d", group, i+1), taskbox.NewTask{Type: taskType, Group: group})
	}
	start := make(chan struct{})
	errs := make(chan error, n)
	var wg sync.WaitGroup
	for _, t := range tasks {
		wg.Go(func() {
			<-start
			errs <- w.sut.enqueueHeld(ctx, t, time.Duration(rand.Int64N(int64(hold)+1)))
		})
	}
	close(start)
	wg.Wait()
	close(errs)
	for err := range errs {
		if err != nil {
			return err
		}
	}
	logf("when: %d concurrent transactions enqueued into group %q (held up to %s)", n, group, hold)
	return nil
}

// --- running: workers and dead-task operations ------------------------------

func registerRunSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^(\d+) workers? runs? for (\S+)$`, w.workersRun)
	sc.Step(`^(\d+) workers? (?:is|are) started$`, w.workersStarted)
	sc.Step(`^the workers are stopped after (\S+)$`, w.workersStopped)
	sc.Step(`^a worker claims "([^"]*)" and stops without an outcome$`, w.workerDies)
	sc.Step(`^task "([^"]*)" is (requeued|cancelled)$`, w.deadTaskOperation)
	sc.Step(`^(\S+) pass$`, w.timePasses)
	sc.Step(`^the retention cleanup runs$`, w.cleanupRuns)
}

func (w *World) workersRun(ctx context.Context, n int, forText string) error {
	d, err := time.ParseDuration(forText)
	if err != nil {
		return err
	}
	runCtx, cancel := context.WithTimeout(ctx, d)
	defer cancel()
	err = taskbox.NewWorker(w.sut.taskStore(), w.registry, w.config(n)).Run(runCtx)
	w.mu.Lock()
	logf("when: %d worker(s) ran for %s -> %d run(s) recorded so far", n, d, len(w.runs))
	w.mu.Unlock()
	return err
}

func (w *World) workersStarted(ctx context.Context, n int) error {
	runCtx, cancel := context.WithCancel(context.WithoutCancel(ctx))
	done := make(chan error, 1)
	go func() { done <- taskbox.NewWorker(w.sut.taskStore(), w.registry, w.config(n)).Run(runCtx) }()
	w.stopWorkers = func() error { cancel(); return <-done }
	logf("when: %d worker(s) started", n)
	return nil
}

func (w *World) workersStopped(ctx context.Context, afterText string) error {
	after, err := time.ParseDuration(afterText)
	if err != nil {
		return err
	}
	if w.stopWorkers == nil {
		return fmt.Errorf("no workers were started")
	}
	time.Sleep(after)
	err = w.stopWorkers()
	w.stopWorkers = nil
	w.mu.Lock()
	logf("when: workers stopped after %s -> %d run(s) recorded so far", after, len(w.runs))
	w.mu.Unlock()
	return err
}

func (w *World) workerDies(ctx context.Context, name string) error {
	id, err := w.alias(name)
	if err != nil {
		return err
	}
	claimed, err := w.sut.taskStore().Claim(ctx, taskbox.DefaultQueue, w.settings.lease, 1)
	if err != nil {
		return err
	}
	logf("when: a worker claimed %d task(s) and abandoned them", len(claimed))
	if len(claimed) != 1 || claimed[0].ID != id {
		return fmt.Errorf("expected to claim %q, claimed %+v", name, claimed)
	}
	return nil
}

func (w *World) deadTaskOperation(ctx context.Context, name, op string) error {
	id, err := w.alias(name)
	if err != nil {
		return err
	}
	if op == "requeued" {
		err = w.sut.taskStore().Requeue(ctx, id)
	} else {
		err = w.sut.taskStore().Cancel(ctx, id)
	}
	logf("when: task %q %s -> err=%v", name, op, err)
	return err
}

func (w *World) timePasses(ctx context.Context, text string) error {
	d, err := time.ParseDuration(text)
	if err != nil {
		return err
	}
	time.Sleep(d)
	logf("when: waited %s", d)
	return nil
}

func (w *World) cleanupRuns(ctx context.Context) error {
	err := w.sut.taskStore().Cleanup(ctx, w.settings.retention)
	logf("when: retention cleanup (default %s) -> err=%v", w.settings.retention, err)
	return err
}

// --- assertions -------------------------------------------------------------

func registerAssertSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the tasks are:$`, w.theTasksAre)
	sc.Step(`^every task is "([^"]*)" after exactly (\d+) runs?$`, w.everyTaskIs)
	sc.Step(`^no task had two runs at the same time$`, w.noOverlappingRuns)
	sc.Step(`^the tasks ran in this order:$`, w.theTasksRanInOrder)
	sc.Step(`^the tasks of group "([^"]*)" ran one at a time in seq order$`, w.groupRanInSeqOrder)
	sc.Step(`^the runs of "([^"]*)" and "([^"]*)" overlapped$`, w.runsOverlapped)
	sc.Step(`^the runs of "([^"]*)" started at least these gaps apart:$`, w.runGaps)
	sc.Step(`^the run of "([^"]*)" attempt (\d+) saw its cancellation$`, w.runSawCancellation)
	sc.Step(`^the first run of "([^"]*)" started no earlier than its run at$`, w.firstRunNotBeforeRunAt)
}

func (w *World) theTasksAre(ctx context.Context, table *godog.Table) error {
	for _, row := range rowsAsMaps(table) {
		name := row["task"]
		id, err := w.alias(name)
		if err != nil {
			return err
		}
		t, err := w.sut.taskStore().Get(ctx, id)
		got := map[string]string{"runs": strconv.Itoa(len(w.runsOf(id)))}
		switch {
		case errors.Is(err, taskbox.ErrNotFound):
			got["status"] = "absent"
		case err != nil:
			return err
		default:
			got["status"] = string(t.Status)
			got["attempt"] = strconv.Itoa(t.Attempt)
			got["last status"] = strconv.Itoa(t.LastStatus)
			got["last error"] = t.LastError
			got["finished"] = map[bool]string{true: "yes", false: "no"}[!t.FinishedAt.IsZero()]
		}
		logf("then: task %q -> %v", name, got)
		for col, want := range row {
			if col == "task" || want == "" {
				continue
			}
			if got[col] != want {
				return fmt.Errorf("task %q %s: got %q, want %q", name, col, got[col], want)
			}
		}
	}
	return nil
}

func (w *World) everyTaskIs(ctx context.Context, status string, runs int) error {
	for _, name := range w.aliases {
		id := w.ids[name]
		t, err := w.sut.taskStore().Get(ctx, id)
		if err != nil {
			return fmt.Errorf("task %q: %w", name, err)
		}
		if string(t.Status) != status || len(w.runsOf(id)) != runs {
			return fmt.Errorf("task %q: status %s after %d run(s), want %s after %d", name, t.Status, len(w.runsOf(id)), status, runs)
		}
	}
	logf("then: all %d task(s) are %s after %d run(s)", len(w.aliases), status, runs)
	return nil
}

func (w *World) noOverlappingRuns(ctx context.Context) error {
	for _, name := range w.aliases {
		rs := w.runsOf(w.ids[name])
		for i := 1; i < len(rs); i++ {
			if rs[i].start.Before(rs[i-1].end) {
				return fmt.Errorf("task %q: run %d started before run %d ended", name, i+1, i)
			}
		}
	}
	logf("then: no overlapping runs among %d task(s)", len(w.aliases))
	return nil
}

func (w *World) theTasksRanInOrder(ctx context.Context, table *godog.Table) error {
	listed := map[string]bool{}
	var want []string
	for _, row := range rowsAsMaps(table) {
		listed[row["task"]] = true
		want = append(want, row["task"])
	}
	type started struct {
		name  string
		start time.Time
	}
	var all []started
	for name := range listed {
		id, err := w.alias(name)
		if err != nil {
			return err
		}
		for _, r := range w.runsOf(id) {
			all = append(all, started{name, r.start})
		}
	}
	sort.Slice(all, func(i, j int) bool { return all[i].start.Before(all[j].start) })
	var got []string
	for _, s := range all {
		got = append(got, s.name)
	}
	logf("then: run order %v, want %v", got, want)
	if strings.Join(got, ",") != strings.Join(want, ",") {
		return fmt.Errorf("run order: got %v, want %v", got, want)
	}
	return nil
}

func (w *World) groupRanInSeqOrder(ctx context.Context, group string) error {
	type seqRun struct {
		name string
		seq  int64
		r    run
	}
	var rs []seqRun
	for _, name := range w.aliases {
		if w.groups[name] != group {
			continue
		}
		t, err := w.sut.taskStore().Get(ctx, w.ids[name])
		if err != nil {
			return err
		}
		for _, r := range w.runsOf(t.ID) {
			rs = append(rs, seqRun{name, t.Seq, r})
		}
	}
	sort.Slice(rs, func(i, j int) bool { return rs[i].r.start.Before(rs[j].r.start) })
	for i := 1; i < len(rs); i++ {
		if rs[i].seq <= rs[i-1].seq {
			return fmt.Errorf("group %q: %s (seq %d) ran after %s (seq %d)", group, rs[i].name, rs[i].seq, rs[i-1].name, rs[i-1].seq)
		}
		if rs[i].r.start.Before(rs[i-1].r.end) {
			return fmt.Errorf("group %q: %s started before %s ended", group, rs[i].name, rs[i-1].name)
		}
	}
	logf("then: group %q ran %d run(s) one at a time in seq order", group, len(rs))
	return nil
}

func (w *World) runsOverlapped(ctx context.Context, a, b string) error {
	ida, err := w.alias(a)
	if err != nil {
		return err
	}
	idb, err := w.alias(b)
	if err != nil {
		return err
	}
	ra, rb := w.runsOf(ida), w.runsOf(idb)
	if len(ra) != 1 || len(rb) != 1 {
		return fmt.Errorf("want one run each, got %d and %d", len(ra), len(rb))
	}
	overlap := ra[0].start.Before(rb[0].end) && rb[0].start.Before(ra[0].end)
	logf("then: %q [%s] and %q [%s] overlapped=%v", a, ra[0].start.Format(time.StampMilli), b, rb[0].start.Format(time.StampMilli), overlap)
	if !overlap {
		return fmt.Errorf("runs of %q and %q did not overlap", a, b)
	}
	return nil
}

func (w *World) runGaps(ctx context.Context, name string, table *godog.Table) error {
	id, err := w.alias(name)
	if err != nil {
		return err
	}
	byAttempt := map[int]run{}
	for _, r := range w.runsOf(id) {
		byAttempt[r.attempt] = r
	}
	for _, row := range rowsAsMaps(table) {
		var from, to int
		if _, err := fmt.Sscanf(row["between attempts"], "%d and %d", &from, &to); err != nil {
			return err
		}
		atLeast, err := time.ParseDuration(row["at least"])
		if err != nil {
			return err
		}
		a, okA := byAttempt[from]
		b, okB := byAttempt[to]
		if !okA || !okB {
			return fmt.Errorf("task %q: no runs for attempts %d and %d", name, from, to)
		}
		gap := b.start.Sub(a.start)
		logf("then: %q attempts %d→%d gap %s (at least %s)", name, from, to, gap, atLeast)
		if gap < atLeast {
			return fmt.Errorf("task %q attempts %d→%d: gap %s, want at least %s", name, from, to, gap, atLeast)
		}
	}
	return nil
}

func (w *World) runSawCancellation(ctx context.Context, name string, attempt int) error {
	id, err := w.alias(name)
	if err != nil {
		return err
	}
	for _, r := range w.runsOf(id) {
		if r.attempt == attempt {
			logf("then: %q attempt %d saw cancellation=%v", name, attempt, r.sawCancel)
			if !r.sawCancel {
				return fmt.Errorf("task %q attempt %d did not see its cancellation", name, attempt)
			}
			return nil
		}
	}
	return fmt.Errorf("task %q has no run with attempt %d", name, attempt)
}

func (w *World) firstRunNotBeforeRunAt(ctx context.Context, name string) error {
	id, err := w.alias(name)
	if err != nil {
		return err
	}
	rs := w.runsOf(id)
	if len(rs) == 0 {
		return fmt.Errorf("task %q never ran", name)
	}
	runAt := w.runAt[name]
	logf("then: %q first run %s, run at %s", name, rs[0].start.Format(time.StampMilli), runAt.Format(time.StampMilli))
	if rs[0].start.Before(runAt) {
		return fmt.Errorf("task %q ran %s before its run at", name, runAt.Sub(rs[0].start))
	}
	return nil
}

// --- helpers ------------------------------------------------------------------

// rowsAsMaps turns a table with a header row into one map per data row.
func rowsAsMaps(table *godog.Table) []map[string]string {
	header := table.Rows[0].Cells
	var out []map[string]string
	for _, row := range table.Rows[1:] {
		m := map[string]string{}
		for i, c := range row.Cells {
			m[header[i].Value] = strings.TrimSpace(c.Value)
		}
		out = append(out, m)
	}
	return out
}

func optDuration(v string) (time.Duration, error) {
	if v == "" {
		return 0, nil
	}
	return time.ParseDuration(v)
}
