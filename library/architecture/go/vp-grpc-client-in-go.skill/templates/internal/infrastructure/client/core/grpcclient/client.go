// Source: skill vp-grpc-client-in-go

package grpcclient

import (
	"context"
	"crypto/tls"
	"fmt"
	"log/slog"
	"net"
	"time"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/credentials"
	"google.golang.org/grpc/credentials/insecure"
	"google.golang.org/grpc/metadata"
	"google.golang.org/grpc/status"
	"google.golang.org/grpc/test/bufconn"

	"{module-path}/internal/infrastructure/client/core/outcome"
	"{module-path}/internal/logging"
)

// RequestIDMetadata carries the request id of the call being served to the API.
const RequestIDMetadata = "x-request-id"

// emulatorBuffer is the size of the in-memory connection to the emulator.
const emulatorBuffer = 1 << 20

// Emulator registers the services of an API, answered inside the process, on
// a gRPC server: the mock of one adapter (internal/infrastructure/client/{api}/mock).
type Emulator func(grpc.ServiceRegistrar)

// Client is the connection to one gRPC API. The adapter builds the generated
// client of each service over it (todo.NewItemsServiceClient(client)); every
// unary call it makes passes the interceptor of this package.
type Client struct {
	*grpc.ClientConn
	// emulator serves the API inside the process when the client is mocked.
	emulator *grpc.Server
}

// New builds the connection to the API cfg names. It does not connect: the
// first call does, so a start does not fail while the API is down. With
// cfg.Mocked() every call is answered by emulator inside the process, over an
// in-memory connection - the same interceptor, log, and outcomes, only no
// network - and a WARN record says so.
func New(cfg Config, logger *slog.Logger, emulator Emulator) (*Client, error) {
	c := &Client{}
	target := cfg.Target()
	options := []grpc.DialOption{grpc.WithUnaryInterceptor(interceptor(cfg, logger))}
	switch {
	case cfg.Mocked():
		listener := bufconn.Listen(emulatorBuffer)
		c.emulator = grpc.NewServer()
		emulator(c.emulator)
		go func() { _ = c.emulator.Serve(listener) }()
		// passthrough: the name is no address to resolve; the dialer
		// connects to the emulator whatever it is.
		target = "passthrough:///" + Mock
		options = append(options,
			grpc.WithContextDialer(func(ctx context.Context, _ string) (net.Conn, error) { return listener.DialContext(ctx) }),
			grpc.WithTransportCredentials(insecure.NewCredentials()))
		logger.Warn("api client mocked", "api", cfg.Name)
	case cfg.TLS:
		options = append(options, grpc.WithTransportCredentials(credentials.NewTLS(&tls.Config{MinVersion: tls.VersionTLS12})))
	default:
		options = append(options, grpc.WithTransportCredentials(insecure.NewCredentials()))
	}
	conn, err := grpc.NewClient(target, options...)
	if err != nil {
		if c.emulator != nil {
			c.emulator.Stop()
		}
		return nil, fmt.Errorf("%s: %w", cfg.Name, err)
	}
	c.ClientConn = conn
	return c, nil
}

// Close closes the connection and stops the emulator of a mocked client.
func (c *Client) Close() error {
	err := c.ClientConn.Close()
	if c.emulator != nil {
		c.emulator.Stop()
	}
	return err
}

// interceptor runs around every unary call of the connection. It ends the
// call after cfg.Timeout at the latest, sends the request id of ctx, and
// writes two records: "grpc client call sent" with the request message as
// the payload, and "grpc client call answered" with the status code, the
// duration, failed, and as the payload the response message or, for a failed
// call, the status received and the request again. It returns ErrUnavailable,
// a *StatusError, or nil.
func interceptor(cfg Config, logger *slog.Logger) grpc.UnaryClientInterceptor {
	return func(ctx context.Context, method string, req, reply any, cc *grpc.ClientConn, invoker grpc.UnaryInvoker, opts ...grpc.CallOption) error {
		ctx, cancel := context.WithTimeout(ctx, cfg.Timeout)
		defer cancel()
		if id := logging.RequestID(ctx); id != "" {
			ctx = metadata.AppendToOutgoingContext(ctx, RequestIDMetadata, id)
		}
		start := time.Now()
		logger.LogAttrs(ctx, slog.LevelInfo, "grpc client call sent",
			slog.String("api", cfg.Name), slog.String("method", method),
			logging.Payload(map[string]any{"request": req}))

		err := invoker(ctx, method, req, reply, cc, opts...)

		answer := status.Convert(err)
		received := map[string]any{"response": reply}
		if err != nil {
			received = map[string]any{"status": answer.Proto(), "request": req}
		}
		logger.LogAttrs(ctx, slog.LevelInfo, "grpc client call answered",
			slog.String("api", cfg.Name), slog.String("method", method),
			slog.String("code", answer.Code().String()), slog.Duration("duration", time.Since(start)),
			logging.Failed(err != nil), logging.Payload(received))

		switch answer.Code() {
		case codes.OK:
			return nil
		case codes.Unavailable, codes.DeadlineExceeded:
			return fmt.Errorf("%w: %s: %s", outcome.ErrUnavailable, method, answer.Message())
		default:
			return &outcome.StatusError{Code: outcome.Code(answer.Code()), Message: answer.Message()}
		}
	}
}
