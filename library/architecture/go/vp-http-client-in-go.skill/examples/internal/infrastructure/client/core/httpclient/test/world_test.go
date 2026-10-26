package test

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"net/http/httptest"
	"net/url"
	"os"
	"strconv"
	"sync"
	"time"

	"github.com/cucumber/godog"

	"github.com/example/todo/internal/infrastructure/client/core/httpclient"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the settings of the test API a scenario may set; each starts with none.
var variables = []string{"TEST_API_HOST", "TEST_API_HTTP_PORT", "TEST_API_TLS", "TEST_API_TIMEOUT"}

// echo is the request and the response of the test API's one operation,
// POST /v1/echo - an API of these tests only, so the fixed code is tested
// without any API of the application.
type echo struct {
	Text string `json:"text"`
}

// received is what the test API was sent.
type received struct {
	method, path, body string
	requestID          string
	hasRequestID       bool
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	// the test API: what it answers, and what it received
	status   int
	body     string
	delay    time.Duration
	down     bool
	server   *httptest.Server
	received []received

	// the client under test
	emulator http.Handler
	mocked   bool
	records  *recorder
	answer   echo
	callErr  error

	// settings.feature
	loaded  httpclient.Config
	loadErr error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{status: http.StatusOK, records: &recorder{}}
		for _, name := range variables {
			if err := os.Unsetenv(name); err != nil {
				return ctx, err
			}
		}
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		if w.server != nil {
			w.server.Close()
		}
		return ctx, nil
	})
}

// testAPI answers every request as the scenario set, recording what it received.
func (w *World) testAPI() http.Handler {
	return http.HandlerFunc(func(rw http.ResponseWriter, r *http.Request) {
		body, _ := io.ReadAll(r.Body)
		id, has := r.Header[httpclient.RequestIDHeader]
		got := received{method: r.Method, path: r.URL.Path, body: string(body), hasRequestID: has}
		if has {
			got.requestID = id[0]
		}
		w.received = append(w.received, got)
		if w.delay > 0 {
			select {
			case <-time.After(w.delay):
			case <-r.Context().Done():
			}
		}
		rw.Header().Set("Content-Type", "application/json")
		rw.WriteHeader(w.status)
		_, _ = io.WriteString(rw, w.body)
	})
}

// client is the client of the test API: against a TCP server of the test API,
// or, mocked, against the scenario's emulator.
func (w *World) client() *httpclient.Client {
	logger := slog.New(w.records)
	if w.mocked {
		return httpclient.New(httpclient.Config{Name: "test-api", Host: httpclient.Mock, Timeout: time.Second}, logger, w.emulator)
	}
	w.server = httptest.NewServer(w.testAPI())
	address, _ := url.Parse(w.server.URL)
	port, _ := strconv.Atoi(address.Port())
	if w.down {
		w.server.Close() // nothing listens on its port any more
	}
	timeout := time.Second
	if w.delay > 0 {
		timeout = 50 * time.Millisecond
	}
	return httpclient.New(httpclient.Config{Name: "test-api", Host: address.Hostname(), Port: port, Timeout: timeout}, logger, nil)
}

// recorder is a slog.Handler keeping every record with all its attributes,
// the payload too.
type recorder struct {
	mu      sync.Mutex
	records []slog.Record
}

func (r *recorder) Enabled(context.Context, slog.Level) bool { return true }

func (r *recorder) Handle(_ context.Context, record slog.Record) error {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.records = append(r.records, record.Clone())
	return nil
}

func (r *recorder) WithAttrs([]slog.Attr) slog.Handler { return r }
func (r *recorder) WithGroup(string) slog.Handler      { return r }

// attrs are the attributes of a record by their key.
func attrs(record slog.Record) map[string]any {
	out := map[string]any{}
	record.Attrs(func(a slog.Attr) bool {
		out[a.Key] = a.Value.Any()
		return true
	})
	return out
}
