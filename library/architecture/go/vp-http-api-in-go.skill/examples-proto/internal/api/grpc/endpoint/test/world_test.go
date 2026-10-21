package test

import (
	"context"
	"fmt"
	"io"
	"log/slog"
	"net"

	"github.com/cucumber/godog"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/test/bufconn"

	"github.com/example/todo/gen/api/todo"
	grpcapi "github.com/example/todo/internal/api/grpc"
	"github.com/example/todo/internal/api/grpc/endpoint"
	grpcpipeline "github.com/example/todo/internal/api/grpc/pipeline"
	"github.com/example/todo/internal/command"
	"github.com/example/todo/internal/command/pipeline"
)

// logf logs a step's action or observation through stdout, so the line stays
// attached to its step in godog's pretty output.
func logf(format string, args ...any) {
	fmt.Printf(format+"\n", args...)
}

// World holds per-scenario state, reset before every scenario. The generic
// behaviour of every method (error mapping, health, shutdown) is tested once
// in internal/api/grpc; here each service is tested for its own mapping.
type World struct {
	// the fake PlanItems handler - the command layer below the gRPC entry point
	planResult command.PlanItemsResult
	planCalled bool
	planGot    command.PlanItems

	planResponse *todo.PlanItemsResponse

	addGot   command.AddItem
	addErr   error
	listGot  command.ListItems
	listHeld []string

	addResponse  *todo.AddItemResponse
	listResponse *todo.ListItemsResponse
	callErr      error

	stop context.CancelFunc
	conn *grpc.ClientConn
}

func (w *World) reset() {
	if w.conn != nil {
		_ = w.conn.Close()
	}
	if w.stop != nil {
		w.stop()
	}
	*w = World{}
}

func registerWorldHooks(sc *godog.ScenarioContext, w *World) {
	sc.Before(func(ctx context.Context, _ *godog.Scenario) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
	sc.After(func(ctx context.Context, _ *godog.Scenario, err error) (context.Context, error) {
		w.reset()
		return ctx, nil
	})
}

// handlers are the fake handlers of every service; a new service adds its fakes here.
func (w *World) handlers() grpcapi.Endpoints {
	return grpcapi.Endpoints{
		Items: endpoint.ItemsHandler{
			PlanItems: pipeline.HandlerFunc[command.PlanItems, command.PlanItemsResult](
				func(_ context.Context, cmd command.PlanItems) (command.PlanItemsResult, error) {
					w.planCalled = true
					w.planGot = cmd
					return w.planResult, nil
				}),
			AddItem: pipeline.HandlerFunc[command.AddItem, command.AddItemResult](
				func(_ context.Context, cmd command.AddItem) (command.AddItemResult, error) {
					w.addGot = cmd
					return command.AddItemResult{Item: command.ItemView{Title: cmd.Title}}, w.addErr
				}),
			ListItems: pipeline.HandlerFunc[command.ListItems, command.ListItemsResult](
				func(_ context.Context, cmd command.ListItems) (command.ListItemsResult, error) {
					w.listGot = cmd
					result := command.ListItemsResult{}
					for _, title := range w.listHeld {
						result.Items = append(result.Items, command.ItemView{Title: title})
					}
					return result, nil
				}),
		},
	}
}

// client serves endpoint.Services on an in-memory listener and returns a
// connection to it, so a scenario calls through the generated client.
func (w *World) client() (*grpc.ClientConn, error) {
	if w.conn != nil {
		return w.conn, nil
	}
	listener := bufconn.Listen(1 << 20)
	ctx, cancel := context.WithCancel(context.Background())
	w.stop = cancel
	logger := slog.New(slog.NewTextHandler(io.Discard, nil))
	server := grpcpipeline.NewServer(grpcpipeline.Config{}, grpcapi.Services(w.handlers()), logger)
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
