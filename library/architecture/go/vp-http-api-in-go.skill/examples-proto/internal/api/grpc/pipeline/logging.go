// Source: skill vp-grpc-api-in-go

package pipeline

import (
	"context"
	"log/slog"
	"time"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/metadata"
	"google.golang.org/grpc/status"

	"github.com/example/todo/internal/logging"
)

// RequestIDMetadata is the metadata key that carries the id of a call: taken
// from the client when it sends one, else made here, and sent back in the
// response header.
const RequestIDMetadata = "x-request-id"

// logCalls writes two records per call, so the log reads top-down as the call
// was served: "grpc call received" when it arrives, with the method and the
// request message as the payload; then "grpc call answered" with the status
// code, the duration, failed (any code but OK), and the response message as
// returned as the payload, so a fault in turning a result into the response
// shows in the log. A failed call's second record repeats the request, so it
// is in the log even when only failed records keep their payload. It puts the
// request id into the call's context, so every record of the call carries it
// once the logging variation point is attached. It runs outside
// recoverPanics, so a panic is logged with the INTERNAL it was answered with.
func logCalls(logger *slog.Logger) grpc.UnaryServerInterceptor {
	return func(ctx context.Context, req any, info *grpc.UnaryServerInfo, handler grpc.UnaryHandler) (any, error) {
		start := time.Now()
		id := ""
		if md, ok := metadata.FromIncomingContext(ctx); ok && len(md.Get(RequestIDMetadata)) > 0 {
			id = md.Get(RequestIDMetadata)[0]
		}
		ctx = logging.WithRequestID(ctx, id)
		_ = grpc.SetHeader(ctx, metadata.Pairs(RequestIDMetadata, logging.RequestID(ctx)))
		logger.LogAttrs(ctx, slog.LevelInfo, "grpc call received",
			slog.String("method", info.FullMethod), logging.Payload(map[string]any{"request": req}))

		resp, err := handler(ctx, req)

		code := status.Code(err)
		sent := map[string]any{"response": resp}
		if code != codes.OK {
			sent["request"] = req
		}
		logger.LogAttrs(ctx, slog.LevelInfo, "grpc call answered",
			slog.String("method", info.FullMethod), slog.String("code", code.String()),
			slog.Duration("duration", time.Since(start)), logging.Failed(code != codes.OK),
			logging.Payload(sent))
		return resp, err
	}
}
