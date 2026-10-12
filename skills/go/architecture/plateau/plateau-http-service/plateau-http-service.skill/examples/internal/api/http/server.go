// Package http is the inbound HTTP adapter: it decodes requests, calls the
// domain service, and encodes the result back as JSON.
package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"

	"github.com/example/linkcheck-service/internal/domain/services"
	"github.com/example/linkcheck-service/internal/version"
)

// Options are the deployment switches of the HTTP adapter.
type Options struct {
	// DocsEnabled registers the spec and Swagger UI routes (HTTP_API_DOCS_ENABLED).
	DocsEnabled bool
	// BasePath is the path prefix the service is published under (HTTP_BASE_PATH):
	// empty, or with a leading "/" and no trailing one. Routes stay mounted at "/";
	// only the URLs handed to a client carry it.
	BasePath string
}

type Server struct {
	service *services.LinkCheckService
	options Options
}

func New(service *services.LinkCheckService, options Options) *Server {
	return &Server{service: service, options: options}
}

type contractRoute struct {
	pattern string
	handler http.HandlerFunc
}

// contractRoutes are the operations of the published contract. The contract is
// written by hand and so are these routes, so they are kept in one list a test
// holds against the contract (ContractRoutes).
func (s *Server) contractRoutes() []contractRoute {
	return []contractRoute{
		{"POST /v1/links/check", s.handleCheck},
	}
}

// ContractRoutes lists the served operations of the published contract as
// "METHOD /path" patterns.
func (s *Server) ContractRoutes() []string {
	patterns := []string{}
	for _, route := range s.contractRoutes() {
		patterns = append(patterns, route.pattern)
	}
	return patterns
}

func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	for _, route := range s.contractRoutes() {
		mux.HandleFunc(route.pattern, route.handler)
	}
	mux.HandleFunc("GET /health", handleHealth)
	mux.HandleFunc("GET /health/live", handleProbe)
	mux.HandleFunc("GET /health/ready", handleProbe)
	if s.options.DocsEnabled {
		registerDocs(mux, s.options.BasePath)
	}
	return mux
}

type healthResponse struct {
	Status  string `json:"status"`
	Version string `json:"version"`
}

// handleHealth reports the running binary's version, so a consumer can tell
// which build it is talking to. It is for clients and people, never a probe.
func handleHealth(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(healthResponse{Status: "ok", Version: version.Version})
}

// handleProbe answers the liveness and readiness probes. This service needs no
// dependency to serve a request, so being ready is the same as being alive.
func handleProbe(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
}

type checkRequest struct {
	URL string `json:"url"`
}

type checkResponse struct {
	URL        string `json:"url"`
	Normalized string `json:"normalized"`
}

func (s *Server) handleCheck(w http.ResponseWriter, r *http.Request) {
	var req checkRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		slog.Error("http: check decode failed", "error", err)
		writeError(w, codeInvalidArgument, err)
		return
	}

	result, err := s.service.Check(r.Context(), req.URL)
	if err != nil {
		code := codeInternal
		if errors.Is(err, services.ErrInvalidURL) {
			code = codeInvalidArgument
		}
		slog.Error("http: check failed", "url", req.URL, "code", code, "error", err)
		writeError(w, code, err)
		return
	}

	// 200: the method created nothing and its work is done.
	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(checkResponse{URL: result.URL, Normalized: result.Normalized})
}
