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

// ReadinessCheck is one dependency /health/ready waits for. The HTTP entry
// point has none of its own: a service variation point (a database, a client)
// adds its check in the composition root.
type ReadinessCheck struct {
	Name  string
	Check func(ctx context.Context) error
}

// Server serves the contract routes, the health endpoints and, when enabled,
// the OpenAPI spec and Swagger UI.
type Server struct {
	cfg     Config
	version string
	routes  []Route
	checks  []ReadinessCheck
	logger  *slog.Logger
}

// NewServer builds the server for the command handlers in handlers; the routes
// come from contractRoutes (routes.go).
func NewServer(cfg Config, version string, handlers Handlers, logger *slog.Logger, checks ...ReadinessCheck) *Server {
	return &Server{cfg: cfg, version: version, routes: contractRoutes(handlers), checks: checks, logger: logger}
}

// ContractRoutes lists the served operations of the published contract as
// "METHOD /path" patterns, for the scenario that holds them against the spec.
func (s *Server) ContractRoutes() []string {
	patterns := make([]string, len(s.routes))
	for i, route := range s.routes {
		patterns[i] = route.Pattern
	}
	return patterns
}

// Handler registers every route on one ServeMux: the contract routes, then the
// routes outside the contract (health, docs).
func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	for _, route := range s.routes {
		mux.Handle(route.Pattern, route.Handler)
	}
	mux.HandleFunc("GET /health/live", s.handleLive)
	mux.HandleFunc("GET /health/ready", s.handleReady)
	mux.HandleFunc("GET /health", s.handleHealth)
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

// handleLive answers whether the process answers at all; it checks no
// dependency, so an outage of a dependency never restarts the service.
func (s *Server) handleLive(w http.ResponseWriter, _ *http.Request) {
	writeJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}

// handleReady answers 200 when every readiness check passes, 503 otherwise,
// naming the checks that failed.
func (s *Server) handleReady(w http.ResponseWriter, r *http.Request) {
	failed := []string{}
	for _, check := range s.checks {
		if err := check.Check(r.Context()); err != nil {
			s.logger.WarnContext(r.Context(), "readiness check failed", "check", check.Name, "error", err)
			failed = append(failed, check.Name)
		}
	}
	if len(failed) > 0 {
		writeJSON(w, http.StatusServiceUnavailable, map[string]any{"status": "unavailable", "failed": failed})
		return
	}
	writeJSON(w, http.StatusOK, map[string]string{"status": "ok"})
}

// handleHealth is for people and clients, never for a probe: the status and
// the version of the running binary.
func (s *Server) handleHealth(w http.ResponseWriter, _ *http.Request) {
	writeJSON(w, http.StatusOK, map[string]string{"status": "ok", "version": s.version})
}
