package test

import (
	"context"
	"fmt"
	"log/slog"
	"net"
	"os"
	"sync"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/metadata"
	"google.golang.org/grpc/status"
	"google.golang.org/protobuf/types/known/wrapperspb"

	"github.com/example/todo/internal/infrastructure/client/core/grpcclient"
	"github.com/example/todo/internal/infrastructure/client/core/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the settings of the test API a scenario may set; each starts with none.
var variables = []string{"TEST_API_HOST", "TEST_API_GRPC_PORT", "TEST_API_TLS", "TEST_API_TIMEOUT"}

// echoMethod is the one method of the test API, an API of these tests only,
// so the fixed code is tested without any API of the application. It takes
// and answers a google.protobuf.StringValue, so it needs no generated code.
const echoMethod = "/test.v1.EchoService/Echo"

// echoService describes the test API by hand, as generated code would; answer
// serves its one method.
func echoService(answer func(ctx context.Context, request *wrapperspb.StringValue) (*wrapperspb.StringValue, error)) (*grpc.ServiceDesc, any) {
	desc := &grpc.ServiceDesc{
		ServiceName: "test.v1.EchoService",
		HandlerType: (*any)(nil),
		Methods: []grpc.MethodDesc{{
			MethodName: "Echo",
			Handler: func(_ any, ctx context.Context, decode func(any) error, _ grpc.UnaryServerInterceptor) (any, error) {
				request := &wrapperspb.StringValue{}
				if err := decode(request); err != nil {
					return nil, err
				}
				return answer(ctx, request)
			},
		}},
	}
	return desc, struct{}{}
}

// received is what the test API was sent.
type received struct {
	text         string
	requestID    string
	hasRequestID bool
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	// the test API: what it answers, and what it received
	text     string
	failure  error
	delay    time.Duration
	down     bool
	server   *grpc.Server
	received []received

	// the client under test
	emulator    grpcclient.Emulator
	middlewares []pipeline.Middleware
	seen        []pipeline.Call
	mocked      bool
	client      *grpcclient.Client
	records     *recorder
	answer      *wrapperspb.StringValue
	callErr     error

	// config.feature
	loaded  grpcclient.Config
	loadErr error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{records: &recorder{}}
		for _, name := range variables {
			if err := os.Unsetenv(name); err != nil {
				return ctx, err
			}
		}
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		if w.client != nil {
			_ = w.client.Close()
		}
		if w.server != nil {
			w.server.Stop()
		}
		return ctx, nil
	})
}

// testAPI answers Echo as the scenario set, recording what it received.
func (w *World) testAPI(ctx context.Context, request *wrapperspb.StringValue) (*wrapperspb.StringValue, error) {
	got := received{text: request.GetValue()}
	if md, ok := metadata.FromIncomingContext(ctx); ok {
		if ids := md.Get(grpcclient.RequestIDMetadata); len(ids) > 0 {
			got.requestID, got.hasRequestID = ids[0], true
		}
	}
	w.received = append(w.received, got)
	if w.delay > 0 {
		select {
		case <-time.After(w.delay):
		case <-ctx.Done():
		}
	}
	if w.failure != nil {
		return nil, w.failure
	}
	return wrapperspb.String(w.text), nil
}

// connect builds the client of the test API: against a TCP gRPC server of the
// test API, or, mocked, against the scenario's emulator.
func (w *World) connect() (err error) {
	logger := slog.New(w.records)
	cfg := grpcclient.Config{Name: "test-api", Host: grpcclient.Mock, Timeout: time.Second}
	if !w.mocked {
		listener, err := net.Listen("tcp", "127.0.0.1:0")
		if err != nil {
			return err
		}
		cfg.Host, cfg.Port = "127.0.0.1", listener.Addr().(*net.TCPAddr).Port
		if w.down {
			_ = listener.Close() // nothing listens on its port any more
		} else {
			w.server = grpc.NewServer()
			w.server.RegisterService(echoService(w.testAPI))
			go func() { _ = w.server.Serve(listener) }()
		}
		if w.delay > 0 {
			cfg.Timeout = 50 * time.Millisecond
		}
	}
	w.client, err = grpcclient.New(cfg, logger, w.emulator, []grpcclient.Method{{Name: echoMethod, Retryable: true}}, w.middlewares)
	return err
}

// codeNamed is the gRPC code of its name in the contract, INVALID_ARGUMENT.
func codeNamed(name string) (codes.Code, error) {
	var code codes.Code
	err := code.UnmarshalJSON([]byte(`"` + name + `"`))
	return code, err
}

// failWith is the status the test API fails with.
func failWith(name, message string) (error, error) {
	code, err := codeNamed(name)
	if err != nil {
		return nil, err
	}
	return status.Error(code, message), nil
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
