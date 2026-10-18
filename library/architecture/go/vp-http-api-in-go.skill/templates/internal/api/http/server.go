// Source: skill vp-http-api-in-go

package http

import (
	"context"
	"errors"
	"log/slog"
	"net"
	"net/http"
	"time"
)

// Route is one operation of the published contract: a "METHOD /path" pattern
// of http.ServeMux and the handler that serves it (usually an Endpoint).
type Route struct {
	Pattern string
	Handler http.Handler
}

// Server serves the contract routes and, when enabled, the OpenAPI spec and
// Swagger UI. The health endpoints are not here: the management entry point
// serves them on a port of its own (skill vp-health-in-go).
type Server struct {
	cfg    Config
	routes []Route
	logger *slog.Logger
}

// Run is the HTTP entry point's one call from the composition root: it reads
// the settings, builds the server, and serves until ctx is cancelled. It
// returns nil after a graceful stop, or the error that stopped the entry
// point. routes are the contract's operations - endpoint.Routes in the
// application's endpoint package.
func Run(ctx context.Context, routes []Route, logger *slog.Logger) error {
	cfg, err := LoadConfig()
	if err != nil {
		return err
	}
	return NewServer(cfg, routes, logger).Serve(ctx)
}

// NewServer builds the server for the contract routes. Tests build it
// directly; the composition root calls Run.
func NewServer(cfg Config, routes []Route, logger *slog.Logger) *Server {
	return &Server{cfg: cfg, routes: routes, logger: logger}
}

// Handler registers every route on one ServeMux: the contract routes, then,
// when enabled, the docs routes outside the contract.
func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	for _, route := range s.routes {
		mux.Handle(route.Pattern, route.Handler)
	}
	if s.cfg.DocsEnabled {
		registerDocs(mux, s.cfg.BasePath)
	}
	return mux
}

// Serve listens on the configured port until ctx is cancelled (SIGTERM), then
// stops accepting requests and waits up to 10 seconds for those in flight.
// It returns nil after a graceful stop, or the error that stopped the server.
func (s *Server) Serve(ctx context.Context) error {
	listener, err := net.Listen("tcp", ":"+s.cfg.ListenPort)
	if err != nil {
		return err
	}
	server := &http.Server{Handler: s.Handler(), ReadHeaderTimeout: 10 * time.Second}
	served := make(chan error, 1)
	go func() { served <- server.Serve(listener) }()
	s.logger.InfoContext(ctx, "http server listening", "addr", listener.Addr().String())

	select {
	case err := <-served:
		return err
	case <-ctx.Done():
		shutdownCtx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()
		if err := server.Shutdown(shutdownCtx); err != nil {
			return err
		}
		if err := <-served; !errors.Is(err, http.ErrServerClosed) {
			return err
		}
		s.logger.Info("http server stopped")
		return nil
	}
}
