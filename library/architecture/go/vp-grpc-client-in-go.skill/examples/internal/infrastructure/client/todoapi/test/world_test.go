package test

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"net"
	"time"

	"github.com/cucumber/godog"
	"google.golang.org/grpc"
	"google.golang.org/protobuf/encoding/protojson"

	"github.com/example/todo/gen/client/todo"
	"github.com/example/todo/internal/infrastructure/client/core/grpcclient"
	"github.com/example/todo/internal/infrastructure/client/todoapi"
)

func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario.
type World struct {
	answer   *todo.PlanItemsResponse
	mocked   bool
	server   *grpc.Server
	client   *todoapi.Client
	method   string
	received *todo.PlanItemsRequest
	items    []todoapi.Item
	callErr  error
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		*w = World{}
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

// testAPI is the todo API as the scenario sets it: it answers PlanItems with
// the scenario's answer and records what it received.
type testAPI struct {
	todo.UnimplementedItemsServiceServer
	w *World
}

func (t testAPI) PlanItems(ctx context.Context, request *todo.PlanItemsRequest) (*todo.PlanItemsResponse, error) {
	t.w.method, _ = grpc.Method(ctx)
	t.w.received = request
	return t.w.answer, nil
}

// connect builds the adapter against a TCP gRPC server of the scenario's todo
// API, or, mocked, against the adapter's own emulator.
func (w *World) connect() error {
	logger := slog.New(slog.NewTextHandler(io.Discard, nil))
	cfg := grpcclient.Config{Name: "todo-api", Host: grpcclient.Mock, Timeout: time.Second}
	if !w.mocked {
		listener, err := net.Listen("tcp", "127.0.0.1:0")
		if err != nil {
			return err
		}
		w.server = grpc.NewServer()
		todo.RegisterItemsServiceServer(w.server, testAPI{w: w})
		go func() { _ = w.server.Serve(listener) }()
		cfg.Host, cfg.Port = "127.0.0.1", listener.Addr().(*net.TCPAddr).Port
	}
	client, err := todoapi.NewClient(cfg, logger, nil)
	w.client = client
	return err
}

func registerSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the todo API answers '([^']*)'$`, func(answer string) error {
		w.answer = &todo.PlanItemsResponse{}
		return protojson.Unmarshal([]byte(answer), w.answer)
	})
	sc.Step(`^the todo API is mocked$`, func() error { w.mocked = true; return nil })
	sc.Step(`^I call PlanItems with the titles "([^"]*)"$`, func(titles string) error {
		if err := w.connect(); err != nil {
			return err
		}
		w.items, w.callErr = w.client.PlanItems(context.Background(), splitTitles(titles))
		logf("when: PlanItems %s -> %+v, error %v", titles, w.items, w.callErr)
		return nil
	})
	sc.Step(`^the todo API should have received "([^"]*)" with '([^']*)'$`, w.shouldHaveReceived)
	sc.Step(`^the items should be "([^"]*)"$`, w.itemsShouldBe)
	registerContractSteps(sc)
}
