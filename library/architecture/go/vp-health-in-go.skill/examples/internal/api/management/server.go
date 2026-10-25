// Source: skill vp-health-in-go

package management

import (
	"context"
	"encoding/json"
	"errors"
	"log/slog"
	"net"
	"net/http"
	"time"

	"github.com/example/todo/internal/api/core/entry"
	"github.com/example/todo/internal/version"
)

// Check is one dependency the service waits for before it takes traffic. The
// management entry point has none of its own: a service variation point (a
// database, a client) adds its check in the composition root.
type Check struct {
	// Name is how a failed check is reported, e.g. "database".
	Name string
	// Check returns nil while the dependency can be used.
	Check func(ctx context.Context) error
}

// Server answers the health endpoints.
type Server struct {
	cfg     Config
	version string
	checks  []Check
	logger  *slog.Logger
}

// Run is the management entry point's one call from the composition root: it
// reads the settings, builds the server, and serves until ctx is cancelled.
// It returns nil after a graceful stop, or the error that stopped it. checks
// are the readiness checks the service variation points add.
func Run(ctx context.Context, logger *slog.Logger, checks ...Check) error {
	cfg, err := LoadConfig()
	if err != nil {
		return err
	}
	return NewServer(cfg, version.Version, logger, checks...).Serve(ctx)
}

// Entry is the management entry point as the composition root lists it for
// entry.RunAll: Run under the name "management". checks are the readiness
// checks the service variation points add.
func Entry(logger *slog.Logger, checks ...Check) entry.Entry {
	return entry.Entry{Name: "management", Run: func(ctx context.Context) error {
		return Run(ctx, logger, checks...)
	}}
}

// NewServer builds the server. Tests build it directly; the composition root
// lists Entry.
func NewServer(cfg Config, version string, logger *slog.Logger, checks ...Check) *Server {
	return &Server{cfg: cfg, version: version, checks: checks, logger: logger}
}

// Handler registers the health endpoints on one ServeMux.
func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /health/live", s.handleLive)
	mux.HandleFunc("GET /health/ready", s.handleReady)
	mux.HandleFunc("GET /health", s.handleHealth)
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
	s.logger.InfoContext(ctx, "management server listening", "addr", listener.Addr().String())

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
		s.logger.Info("management server stopped")
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

func writeJSON(w http.ResponseWriter, status int, body any) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(body)
}
