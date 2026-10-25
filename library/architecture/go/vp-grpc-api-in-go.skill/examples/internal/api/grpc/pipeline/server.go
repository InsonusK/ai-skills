// Source: skill vp-grpc-api-in-go

package pipeline

import (
	"context"
	"log/slog"
	"net"
	"time"

	"google.golang.org/grpc"
	"google.golang.org/grpc/reflection"
)

// Service is one service of the published contract: its generated descriptor
// and the implementation of its generated server interface.
type Service struct {
	// Desc is the generated descriptor, e.g. &todo.ItemsService_ServiceDesc.
	Desc *grpc.ServiceDesc
	// Impl implements the generated server interface, e.g. todo.ItemsServiceServer.
	Impl any
}

// Interceptor is a decorator of the calls of the contract's methods - the
// gRPC form of an entry decorator (skill base-plateau-extension), e.g. one
// that reads the caller's credentials from the metadata into the call's
// context. It rejects a call by returning an error of a kind
// (commandpipeline.ErrUnauthenticated), answered by its kind like a handler's.
type Interceptor = grpc.UnaryServerInterceptor

// Server serves the contract's services and, when enabled, server reflection.
// It serves no health service: the probes of every service, whatever its
// entry points, go to the same HTTP health endpoints.
type Server struct {
	cfg          Config
	services     []Service
	logger       *slog.Logger
	interceptors []Interceptor
}

// Run is the gRPC entry point's one call from the composition root: it reads
// the settings, builds the server, and serves until ctx is cancelled. It
// returns nil after a graceful stop, or the error that stopped the entry
// point. services are the contract's services - Services in the
// application's api/grpc package; interceptors wrap every call, the first the
// outermost.
func Run(ctx context.Context, services []Service, logger *slog.Logger, interceptors []Interceptor) error {
	cfg, err := LoadConfig()
	if err != nil {
		return err
	}
	return NewServer(cfg, services, logger, interceptors).Serve(ctx)
}

// NewServer builds the server for the contract's services. Tests build it
// directly; the composition root calls Run.
func NewServer(cfg Config, services []Service, logger *slog.Logger, interceptors []Interceptor) *Server {
	return &Server{cfg: cfg, services: services, logger: logger, interceptors: interceptors}
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
// one grpc.Server. Every call goes through the fixed interceptors - the call
// log, the panic recovery, the answer by kind - and then through the
// application's, so a call an interceptor rejects is logged and answered like
// any other.
func (s *Server) build() *grpc.Server {
	chain := append([]Interceptor{logCalls(s.logger), recoverPanics(s.logger), answerKinds}, s.interceptors...)
	server := grpc.NewServer(grpc.ChainUnaryInterceptor(chain...))
	for _, service := range s.services {
		server.RegisterService(service.Desc, service.Impl)
	}
	if s.cfg.ReflectionEnabled {
		reflection.Register(server)
	}
	return server
}
