// Source: skill vp-http-api-in-go

package pipeline

import (
	"context"
	"errors"
	"log/slog"
	"net"
	"net/http"
	"time"

	"{module-path}/internal/api/http/middleware/answerfailures"
	"{module-path}/internal/api/http/middleware/requestlog"
)

// Route is one operation of the published contract: a "METHOD /path" pattern
// of http.ServeMux and the handler that serves it (usually an Endpoint).
type Route struct {
	Pattern string
	Handler http.Handler
}

// Middleware is a decorator of the requests of the contract's routes - the
// HTTP form of an entry decorator (skill base-plateau-extension): it takes the
// next handler and returns one that does its step around it, e.g. reads the
// caller's credentials into the request's context. It rejects a request by
// answering an error of a kind with answerfailures.WriteError, never with an
// answer of its own, so the client gets google.rpc.Status like from any route.
// Each lives in a package of its own under internal/api/http/middleware/.
type Middleware func(next http.Handler) http.Handler

// Server serves the contract routes and, when enabled, the OpenAPI spec and
// Swagger UI. The health endpoints are not here: the management entry point
// serves them on a port of its own (skill vp-health-in-go).
type Server struct {
	cfg         Config
	routes      []Route
	logger      *slog.Logger
	middlewares []Middleware
}

// Run is the HTTP entry point's one call from the composition root: it reads
// the settings, builds the server, and serves until ctx is cancelled. It
// returns nil after a graceful stop, or the error that stopped the entry
// point. routes are the contract's operations - Routes in the application's
// api/http package; middlewares wrap each of them, the first the outermost.
func Run(ctx context.Context, routes []Route, logger *slog.Logger, middlewares []Middleware) error {
	cfg, err := LoadConfig()
	if err != nil {
		return err
	}
	return NewServer(cfg, routes, logger, middlewares).Serve(ctx)
}

// NewServer builds the server for the contract routes. Tests build it
// directly; the composition root calls Run.
func NewServer(cfg Config, routes []Route, logger *slog.Logger, middlewares []Middleware) *Server {
	return &Server{cfg: cfg, routes: routes, logger: logger, middlewares: middlewares}
}

// Handler registers every route on one ServeMux: the contract routes, each
// wrapped by the middlewares, then, when enabled, the docs routes outside the
// contract and outside the middlewares. The fixed middleware wrap them all -
// the request log outermost, then answerfailures - so a request a middleware
// rejects, or one whose handler panicked, is answered and logged like any
// other.
func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	for _, route := range s.routes {
		mux.Handle(route.Pattern, s.decorate(route.Handler))
	}
	if s.cfg.DocsEnabled {
		registerDocs(mux, s.cfg.BasePath)
	}
	return requestlog.Middleware(s.logger)(answerfailures.Middleware(s.logger)(mux))
}

// decorate wraps one contract route with the middlewares: the last wraps the
// route first, so the first of the list ends up outermost.
func (s *Server) decorate(route http.Handler) http.Handler {
	for i := len(s.middlewares) - 1; i >= 0; i-- {
		route = s.middlewares[i](route)
	}
	return route
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
