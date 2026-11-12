package main

// The application over its database (features/persistence.feature): run, the
// composition root's own function, started and stopped in the process, on a
// database of its own per scenario on TEST_DATABASE_DSN's server.

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"io"
	"net"
	"net/http"
	"os"
	"strconv"
	"strings"
	"sync"
	"testing"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/postgres"
	"github.com/example/todo/internal/infrastructure/postgres/migrations"
	"github.com/example/todo/internal/infrastructure/postgres/postgrestest"
)

// app is the running application of a scenario and what it wrote.
type app struct {
	database *postgrestest.Database
	env      map[string]string
	logs     syncBuffer
	httpPort string
	stop     context.CancelFunc
	ended    chan int
	exitCode int
	status   int
	body     string
}

type syncBuffer struct {
	mu  sync.Mutex
	buf bytes.Buffer
}

func (b *syncBuffer) Write(p []byte) (int, error) {
	b.mu.Lock()
	defer b.mu.Unlock()
	return b.buf.Write(p)
}

func (b *syncBuffer) String() string {
	b.mu.Lock()
	defer b.mu.Unlock()
	return b.buf.String()
}

func freePort() string {
	l, _ := net.Listen("tcp", "127.0.0.1:0")
	defer l.Close()
	return strconv.Itoa(l.Addr().(*net.TCPAddr).Port)
}

// start runs the application with the scenario's environment until stop.
func (a *app) start() {
	for name, value := range a.env {
		os.Setenv(name, value)
	}
	ctx, cancel := context.WithCancel(context.Background())
	a.stop, a.ended = cancel, make(chan int, 1)
	go func() { a.ended <- run(ctx, io.Discard, &a.logs) }()
}

func (a *app) waitListening() error {
	for range 200 {
		if conn, err := net.Dial("tcp", "127.0.0.1:"+a.httpPort); err == nil {
			conn.Close()
			return nil
		}
		select {
		case code := <-a.ended:
			return fmt.Errorf("the application ended with %d: %s", code, a.logs.String())
		case <-time.After(25 * time.Millisecond):
		}
	}
	return fmt.Errorf("the application does not listen on %s", a.httpPort)
}

func (a *app) send(method, path, body string) error {
	req, err := http.NewRequest(method, "http://127.0.0.1:"+a.httpPort+path, strings.NewReader(body))
	if err != nil {
		return err
	}
	if body != "" {
		req.Header.Set("Content-Type", "application/json")
	}
	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()
	answered, _ := io.ReadAll(resp.Body)
	a.status, a.body = resp.StatusCode, string(answered)
	fmt.Printf("when: %s %s -> %d %s\n", method, path, a.status, a.body)
	return nil
}

func (a *app) records() []map[string]any {
	found := []map[string]any{}
	for _, line := range strings.Split(strings.TrimSpace(a.logs.String()), "\n") {
		var rec map[string]any
		if json.Unmarshal([]byte(line), &rec) == nil {
			found = append(found, rec)
		}
	}
	return found
}

func registerPersistenceSteps(sc *godog.ScenarioContext, a *app) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*a = app{env: map[string]string{
			"DATABASE_DSN": "", "MIGRATE_ON_START": "", "HTTP_LISTEN_PORT": freePort(), "GRPC_LISTEN_PORT": freePort(),
		}}
		a.httpPort = a.env["HTTP_LISTEN_PORT"]
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		if a.stop != nil {
			a.stop()
			<-a.ended
		}
		for name := range a.env {
			os.Unsetenv(name)
		}
		if a.database != nil {
			return ctx, a.database.Drop(context.Background())
		}
		return ctx, nil
	})
	newDatabase := func(migrated bool) error {
		db, err := postgrestest.Create(context.Background())
		if err != nil {
			return err
		}
		a.database, a.env["DATABASE_DSN"] = db, db.DSN
		if !migrated {
			return nil
		}
		opened, err := postgres.Open(db.Config)
		if err != nil {
			return err
		}
		defer opened.Close()
		return postgres.Migrate(context.Background(), opened, migrations.Files)
	}
	sc.Step(`^a migrated database for the application$`, func() error { return newDatabase(true) })
	sc.Step(`^an empty database for the application$`, func() error { return newDatabase(false) })
	sc.Step(`^a database for the application that does not answer$`, func() error {
		a.env["DATABASE_DSN"] = "postgres://nobody@127.0.0.1:1/nothing?connect_timeout=2"
		return nil
	})
	sc.Step(`^the application migrates on start$`, func() error { a.env["MIGRATE_ON_START"] = "true"; return nil })
	sc.Step(`^the application is running$`, func() error { a.start(); return a.waitListening() })
	sc.Step(`^the application restarts$`, func() error {
		a.stop()
		<-a.ended
		a.start()
		return a.waitListening()
	})
	add := func(title, list string) error {
		return a.send(http.MethodPost, "/v1/lists/"+list+"/items", `{"title":"`+title+`"}`)
	}
	sc.Step(`^"([^"]*)" was added to the list "([^"]*)" over HTTP$`, func(title, list string) error {
		if err := add(title, list); err != nil {
			return err
		}
		if a.status != http.StatusCreated {
			return fmt.Errorf("add: %d %s", a.status, a.body)
		}
		return nil
	})
	sc.Step(`^I add "([^"]*)" to the list "([^"]*)" over HTTP$`, add)
	sc.Step(`^I read the list "([^"]*)" over HTTP$`, func(list string) error {
		return a.send(http.MethodGet, "/v1/lists/"+list+"/items", "")
	})
	sc.Step(`^the application should answer (\d+)$`, func(want int) error {
		if a.status != want {
			return fmt.Errorf("status: got %d, want %d (%s)", a.status, want, a.body)
		}
		return nil
	})
	sc.Step(`^the answer should mention "([^"]*)"$`, func(text string) error {
		if !strings.Contains(a.body, text) {
			return fmt.Errorf("answer %s does not mention %q", a.body, text)
		}
		return nil
	})
	sc.Step(`^the application starts and ends$`, func() error {
		a.start()
		select {
		case a.exitCode = <-a.ended:
			a.stop = nil
			return nil
		case <-time.After(30 * time.Second):
			return fmt.Errorf("the application did not end: %s", a.logs.String())
		}
	})
	sc.Step(`^the application should have exited with (\d+)$`, func(want int) error {
		if a.exitCode != want {
			return fmt.Errorf("exit code: got %d, want %d (%s)", a.exitCode, want, a.logs.String())
		}
		return nil
	})
	sc.Step(`^the log should mention "([^"]*)"$`, func(text string) error {
		if !strings.Contains(a.logs.String(), text) {
			return fmt.Errorf("the log does not mention %q: %s", text, a.logs.String())
		}
		return nil
	})
	sc.Step(`^the log should have no error record$`, func() error {
		for _, rec := range a.records() {
			if rec["level"] == "ERROR" {
				return fmt.Errorf("an error record: %v", rec)
			}
		}
		return nil
	})
}

func TestPersistence(t *testing.T) {
	a := &app{}
	suite := godog.TestSuite{
		ScenarioInitializer: func(sc *godog.ScenarioContext) { registerPersistenceSteps(sc, a) },
		Options: &godog.Options{
			Format: "pretty", Paths: []string{"features"}, Tags: "~@status/todo && ~@status/broken",
			Strict: true, TestingT: t,
		},
	}
	if suite.Run() != 0 {
		t.Fatal("non-zero status returned, failed to run feature tests")
	}
}
