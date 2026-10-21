// Source: skill vp-grpc-api-in-go

package grpc

import (
	"context"
	"log/slog"
	"net"
	"time"

	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/metadata"
	"google.golang.org/grpc/reflection"
	"google.golang.org/grpc/status"

	"{module-path}/internal/logging"
)

// Service is one service of the published contract: its generated descriptor
// and the implementation of its generated server interface.
type Service struct {
	// Desc is the generated descriptor, e.g. &todo.ItemsService_ServiceDesc.
	Desc *grpc.ServiceDesc
	// Impl implements the generated server interface, e.g. todo.ItemsServiceServer.
	Impl any
}

// Server serves the contract's services and, when enabled, server reflection.
// It serves no health service: the probes of every service, whatever its
// entry points, go to the same HTTP health endpoints.
type Server struct {
	cfg      Config
	services []Service
	logger   *slog.Logger
}

// Run is the gRPC entry point's one call from the composition root: it reads
// the settings, builds the server, and serves until ctx is cancelled. It
// returns nil after a graceful stop, or the error that stopped the entry
// point. services are the contract's services - endpoint.Services in the
// application's endpoint package.
func Run(ctx context.Context, services []Service, logger *slog.Logger) error {
	cfg, err := LoadConfig()
	if err != nil {
		return err
	}
	return NewServer(cfg, services, logger).Serve(ctx)
}

// NewServer builds the server for the contract's services. Tests build it
// directly; the composition root calls Run.
func NewServer(cfg Config, services []Service, logger *slog.Logger) *Server {
	return &Server{cfg: cfg, services: services, logger: logger}
}

// Serve listens on the configured port until ctx is cancelled (SIGTERM).
func (s *Server) Serve(ctx context.Context) error {
	listener, err := net.Listen("tcp", ":"+s.cfg.ListenPort)
	if err != nil {
		return err
	}
	return s.ServeListener(ctx, listener)
}

// ServeListener serves on a listener the caller opened - Serve passes a TCP
// one, tests an in-memory one. When ctx is cancelled it stops accepting calls
// and waits up to 10 seconds for those in flight, then cuts the rest off. It
// returns nil after the stop, or the error that stopped the server.
func (s *Server) ServeListener(ctx context.Context, listener net.Listener) error {
	server := s.build()
	served := make(chan error, 1)
	go func() { served <- server.Serve(listener) }()
	s.logger.InfoContext(ctx, "grpc server listening", "addr", listener.Addr().String())

	select {
	case err := <-served:
		return err
	case <-ctx.Done():
		stopped := make(chan struct{})
		go func() {
			server.GracefulStop()
			close(stopped)
		}()
		select {
		case <-stopped:
		case <-time.After(10 * time.Second):
			server.Stop()
		}
		<-served
		s.logger.Info("grpc server stopped")
		return nil
	}
}

// build registers the contract's services and, when enabled, reflection on
// one grpc.Server.
func (s *Server) build() *grpc.Server {
	server := grpc.NewServer(grpc.ChainUnaryInterceptor(logCalls(s.logger), recoverPanics(s.logger)))
	for _, service := range s.services {
		server.RegisterService(service.Desc, service.Impl)
	}
	if s.cfg.ReflectionEnabled {
		reflection.Register(server)
	}
	return server
}

// RequestIDMetadata is the metadata key that carries the id of a call: taken
// from the client when it sends one, else made here, and sent back in the
// response header.
const RequestIDMetadata = "x-request-id"

// logCalls writes one record per call once it is answered - "grpc call" with
// the method, the status code, the duration, failed (any code but OK), and as
// the payload the request message and the response message as returned, so a
// fault in turning a result into the response shows in the log. It puts the request id into the call's
// context, so every record of the call carries it once the logging variation
// point is attached. It runs outside recoverPanics, so a panic is logged with
// the INTERNAL it was answered with.
func logCalls(logger *slog.Logger) grpc.UnaryServerInterceptor {
	return func(ctx context.Context, req any, info *grpc.UnaryServerInfo, handler grpc.UnaryHandler) (any, error) {
		start := time.Now()
		id := ""
		if md, ok := metadata.FromIncomingContext(ctx); ok && len(md.Get(RequestIDMetadata)) > 0 {
			id = md.Get(RequestIDMetadata)[0]
		}
		ctx = logging.WithRequestID(ctx, id)
		_ = grpc.SetHeader(ctx, metadata.Pairs(RequestIDMetadata, logging.RequestID(ctx)))
		resp, err := handler(ctx, req)
		code := status.Code(err)
		logger.LogAttrs(ctx, slog.LevelInfo, "grpc call",
			slog.String("method", info.FullMethod), slog.String("code", code.String()),
			slog.Duration("duration", time.Since(start)), logging.Failed(code != codes.OK),
			logging.Payload(map[string]any{"request": req, "response": resp}))
		return resp, err
	}
}

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
