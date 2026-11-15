// Source: skill vp-grpc-api-in-go

// Package answerfailures is the fixed gRPC middleware that answers every
// failure of a call by its status: an error of a kind by the kind's code
// (StatusOf), a panic as INTERNAL. An error of no kind is answered INTERNAL
// "internal error" and logged as "grpc call failed" with its text, so the
// operator reads why. The server puts it inside the call log and outside the
// application's middleware; Call returns a command's error to it as it is.
package answerfailures

import (
	"context"
	"log/slog"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"github.com/example/todo/internal/api/core/rpcstatus"
	"github.com/example/todo/internal/logging"
)

// Middleware answers a method that panicked with INTERNAL and logs the panic
// - unlike net/http, grpc-go does not recover a panic in a method, and one
// would stop the whole process - and an error that is no status by its kind,
// so an application's middleware rejects a call with an error of a kind
// (pipeline.ErrUnauthenticated) instead of building a status itself. An error
// that already is a status passes unchanged.
func Middleware(logger *slog.Logger) grpc.UnaryServerInterceptor {
	return func(ctx context.Context, req any, info *grpc.UnaryServerInfo, handler grpc.UnaryHandler) (resp any, err error) {
		defer func() {
			if p := recover(); p != nil {
				logger.ErrorContext(ctx, "grpc method panicked", "method", info.FullMethod, "panic", p)
				resp, err = nil, status.Error(codes.Internal, rpcstatus.InternalMessage)
			}
		}()
		resp, err = handler(ctx, req)
		if _, isStatus := status.FromError(err); err == nil || isStatus {
			return resp, err
		}
		answer := StatusOf(err)
		if status.Code(answer) == codes.Internal {
			// The caller gets "internal error"; the operator reads why.
			logger.LogAttrs(ctx, slog.LevelError, "grpc call failed", slog.String("method", info.FullMethod), logging.Error(err))
		}
		return resp, answer
	}
}

// StatusOf answers an error by its kind through the one table of rpcstatus:
// a kind's code and message, or INTERNAL "internal error" for an error with
// no kind.
func StatusOf(err error) error {
	code, message := rpcstatus.Of(err)
	return status.Error(codes.Code(code), message)
}
