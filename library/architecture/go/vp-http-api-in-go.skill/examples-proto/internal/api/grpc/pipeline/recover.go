// Source: skill vp-grpc-api-in-go

package pipeline

import (
	"context"
	"log/slog"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

// recoverPanics answers a method that panicked with INTERNAL and logs the
// panic. Unlike net/http, grpc-go does not recover a panic in a method, and
// one would stop the whole process.
func recoverPanics(logger *slog.Logger) grpc.UnaryServerInterceptor {
	return func(ctx context.Context, req any, info *grpc.UnaryServerInfo, handler grpc.UnaryHandler) (resp any, err error) {
		defer func() {
			if p := recover(); p != nil {
				logger.ErrorContext(ctx, "grpc method panicked", "method", info.FullMethod, "panic", p)
				resp, err = nil, status.Error(codes.Internal, "internal error")
			}
		}()
		return handler(ctx, req)
	}
}
