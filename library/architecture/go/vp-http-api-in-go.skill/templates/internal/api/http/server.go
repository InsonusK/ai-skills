// Source: skill vp-http-api-in-go

package http

import (
	"bytes"
	"context"
	"errors"
	"io"
	"log/slog"
	"net"
	"net/http"
	"time"

	"{module-path}/internal/logging"
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
	return s.logRequests(mux)
}

// RequestIDHeader carries the id of a request: taken from the client when it
// sends one, else made here, and always sent back in the response.
const RequestIDHeader = "X-Request-Id"

// logRequests writes one record per request once it is answered - "http
// request" with the method, the path, the status, the duration, failed (a
// status of 400 or above), and as the payload what the client sent: the
// query, the headers, and the body. It puts the request id into the request's
// context, so every record of the request carries it once the logging
// variation point is attached. The body is read here and given back to the
// routes unchanged.
func (s *Server) logRequests(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		ctx := logging.WithRequestID(r.Context(), r.Header.Get(RequestIDHeader))
		w.Header().Set(RequestIDHeader, logging.RequestID(ctx))
		body, _ := io.ReadAll(r.Body) // a broken body reaches the route empty and fails its decode
		r.Body = io.NopCloser(bytes.NewReader(body))
		answered := &statusRecorder{ResponseWriter: w, status: http.StatusOK}
		next.ServeHTTP(answered, r.WithContext(ctx))
		s.logger.LogAttrs(ctx, slog.LevelInfo, "http request",
			slog.String("method", r.Method), slog.String("path", r.URL.Path),
			slog.Int("status", answered.status), slog.Duration("duration", time.Since(start)),
			logging.Failed(answered.status >= http.StatusBadRequest),
			logging.Payload(map[string]any{"query": r.URL.Query(), "headers": r.Header, "body": body}))
	})
}

// statusRecorder remembers the status a route answered with.
type statusRecorder struct {
	http.ResponseWriter
	status int
}

func (r *statusRecorder) WriteHeader(status int) {
	r.status = status
	r.ResponseWriter.WriteHeader(status)
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
