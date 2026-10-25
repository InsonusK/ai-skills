// Source: skill vp-grpc-api-in-go

// Package answerfailures is the fixed gRPC middleware that answers every
// failure of a call by its status: an error of a kind by the kind's code
// (StatusOf, which Call uses too), a panic as INTERNAL. The server puts it
// inside the call log and outside the application's middleware.
package answerfailures

import (
	"context"
	"log/slog"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"{module-path}/internal/api/core/rpcstatus"
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
		if _, isStatus := status.FromError(err); !isStatus {
			return resp, StatusOf(err)
		}
		return resp, err
	}
}

// StatusOf answers an error by its kind through the one table of rpcstatus:
// a kind's code and message, or INTERNAL "internal error" for an error with
// no kind.
func StatusOf(err error) error {
	code, message := rpcstatus.Of(err)
	return status.Error(codes.Code(code), message)
}
