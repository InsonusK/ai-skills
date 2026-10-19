package test

import (
	"context"
	"errors"
	"fmt"
	"io"
	"log/slog"
	"net"
	"os"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/test/bufconn"
	"google.golang.org/protobuf/types/known/wrapperspb"

	grpcapi "github.com/example/todo/internal/api/grpc"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// variables are the environment variables a scenario may set; each scenario starts with none.
var variables = []string{"GRPC_LISTEN_PORT", "GRPC_REFLECTION_ENABLED"}

// World holds per-scenario state, reset before every scenario.
type World struct {
	// the fake echo handler - a command of these tests only, so the generic
	// entry point is tested without any command of the application
	echoAnswer  string
	echoErr     error
	echoPanics  bool
	echoDelay   time.Duration
	echoCalled  bool
	echoGot     echoCommand
	echoStarted chan struct{}

	cfg grpcapi.Config

	// the in-memory server, started by the first call of a scenario
	stopInMemory context.CancelFunc
	conn         *grpc.ClientConn

	reply   string
	listed  []string
	callErr error

	loadedCfg grpcapi.Config
	loadErr   error

	// the TCP server of the lifecycle scenarios
	port     string
	stop     context.CancelFunc
	served   chan error
	occupied net.Listener
	inFlight chan callResult
}

type callResult struct {
	reply string
	err   error
}

func (w *World) reset() error {
	if w.conn != nil {
		_ = w.conn.Close()
	}
	if w.stopInMemory != nil {
		w.stopInMemory()
	}
	if w.stop != nil {
		w.stop()
	}
	if w.occupied != nil {
		_ = w.occupied.Close()
	}
	*w = World{echoStarted: make(chan struct{}, 1)}
	for _, name := range variables {
		if err := os.Unsetenv(name); err != nil {
			return err
		}
	}
	return nil
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		return ctx, w.reset()
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		return ctx, w.reset()
	})
}

// echoCommand and echoResult stand for any command and its result.
type echoCommand struct{ Text string }
type echoResult struct{ Text string }

// echoHandler is the fake handler: it records the command and answers what the scenario set.
func (w *World) echoHandler() pipeline.Handler[echoCommand, echoResult] {
	return pipeline.HandlerFunc[echoCommand, echoResult](
		func(_ context.Context, cmd echoCommand) (echoResult, error) {
			w.echoCalled = true
			w.echoGot = cmd
			select {
			case w.echoStarted <- struct{}{}:
			default:
			}
			if w.echoPanics {
				panic("the command panicked")
			}
			time.Sleep(w.echoDelay)
			return echoResult{Text: w.echoAnswer}, w.echoErr
		})
}

// echoServer is the one test service, test.v1.EchoService. Its messages are
// the well-known StringValue, so the tests need no proto file of their own.
type echoServer struct{ w *World }

func (s echoServer) echo(ctx context.Context, req *wrapperspb.StringValue) (*wrapperspb.StringValue, error) {
	decode := func(req *wrapperspb.StringValue) (echoCommand, error) {
		if req.GetValue() == "" {
			return echoCommand{}, errors.New("text is required")
		}
		return echoCommand{Text: req.GetValue()}, nil
	}
	encode := func(result echoResult) *wrapperspb.StringValue { return wrapperspb.String(result.Text) }
	return grpcapi.Call(ctx, s.w.echoHandler(), req, decode, encode)
}

// echoDesc is what protoc-gen-go-grpc would generate for
// `service EchoService { rpc Echo(StringValue) returns (StringValue); }`.
var echoDesc = grpc.ServiceDesc{
	ServiceName: "test.v1.EchoService",
	HandlerType: (*any)(nil),
	Methods: []grpc.MethodDesc{{
		MethodName: "Echo",
		Handler: func(srv any, ctx context.Context, dec func(any) error, interceptor grpc.UnaryServerInterceptor) (any, error) {
			in := new(wrapperspb.StringValue)
			if err := dec(in); err != nil {
				return nil, err
			}
			call := func(ctx context.Context, req any) (any, error) {
				return srv.(echoServer).echo(ctx, req.(*wrapperspb.StringValue))
			}
			info := &grpc.UnaryServerInfo{Server: srv, FullMethod: echoMethod}
			return interceptor(ctx, in, info, call)
		},
	}},
}

const echoMethod = "/test.v1.EchoService/Echo"

func (w *World) services() []grpcapi.Service {
	return []grpcapi.Service{{Desc: &echoDesc, Impl: echoServer{w: w}}}
}

func discard() *slog.Logger {
	return slog.New(slog.NewTextHandler(io.Discard, nil))
}

// client starts the in-memory server on the scenario's first call, with the
// settings its Given steps set, and returns a connection to it.
func (w *World) client() (*grpc.ClientConn, error) {
	if w.conn != nil {
		return w.conn, nil
	}
	listener := bufconn.Listen(1 << 20)
	ctx, cancel := context.WithCancel(context.Background())
	w.stopInMemory = cancel
	server := grpcapi.NewServer(w.cfg, w.services(), discard())
	go func() { _ = server.ServeListener(ctx, listener) }()
	conn, err := grpc.NewClient("passthrough:///in-memory",
		grpc.WithContextDialer(func(ctx context.Context, _ string) (net.Conn, error) { return listener.DialContext(ctx) }),
		grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return nil, err
	}
	w.conn = conn
	return conn, nil
}

// callEcho calls the test method over conn.
func callEcho(conn *grpc.ClientConn, text string) (string, error) {
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	out := new(wrapperspb.StringValue)
	err := conn.Invoke(ctx, echoMethod, wrapperspb.String(text), out)
	return out.GetValue(), err
}
