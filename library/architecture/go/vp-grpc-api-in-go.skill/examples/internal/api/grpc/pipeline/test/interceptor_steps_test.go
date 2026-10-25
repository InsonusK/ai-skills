package test

import (
	"context"
	"fmt"
	"log/slog"

	"github.com/cucumber/godog"
	"google.golang.org/grpc"

	grpcpipeline "github.com/example/todo/internal/api/grpc/pipeline"
	"github.com/example/todo/internal/command/pipeline"
)

func registerInterceptorSteps(sc *godog.ScenarioContext, w *World) {
	sc.Step(`^the interceptors "([^"]*)" and "([^"]*)", each logging a record with its name$`, w.theInterceptorsLogging)
	sc.Step(`^an interceptor that rejects every call as unauthenticated with "([^"]*)"$`, w.aRejectingInterceptor)
}

// theInterceptorsLogging gives the server two interceptors, in this order,
// each writing the record "interceptor {name}" before it passes the call on.
func (w *World) theInterceptorsLogging(outer, inner string) error {
	logger := slog.New(slog.NewJSONHandler(&w.logs, nil))
	for _, name := range []string{outer, inner} {
		w.interceptors = append(w.interceptors, func(ctx context.Context, req any, _ *grpc.UnaryServerInfo, handler grpc.UnaryHandler) (any, error) {
			logger.InfoContext(ctx, "interceptor "+name)
			return handler(ctx, req)
		})
	}
	return nil
}

// aRejectingInterceptor gives the server an interceptor that rejects every
// call with an unauthenticated error - a plain error of a kind, not a status,
// as an authentication interceptor rejects a call without credentials.
func (w *World) aRejectingInterceptor(message string) error {
	w.interceptors = append(w.interceptors, grpcpipeline.Interceptor(
		func(context.Context, any, *grpc.UnaryServerInfo, grpc.UnaryHandler) (any, error) {
			return nil, fmt.Errorf("%w: %s", pipeline.ErrUnauthenticated, message)
		}))
	return nil
}
