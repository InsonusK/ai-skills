// Package http is the inbound HTTP adapter: the HTTP form of the service's
// grpc contract. It decodes a request into the contract's generated message,
// calls the domain service, and encodes the result back as proto-JSON.
package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"
	"strconv"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "github.com/example/linkcheck-service/gen/api/linkcheck"
	"github.com/example/linkcheck-service/internal/domain/interfaces"
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
// generated from the proto file and these routes are written by hand, so they
// are kept in one list a test holds against the contract (ContractRoutes).
func (s *Server) contractRoutes() []contractRoute {
	return []contractRoute{
		{"POST /v1/links/check", s.handleCheck},
		{"GET /v1/links/recent", s.handleRecent},
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
	if s.options.DocsEnabled {
		registerDocs(mux, s.options.BasePath)
	}
	return mux
}

type healthResponse struct {
	Status  string `json:"status"`
	Version string `json:"version"`
}

// handleHealth reports liveness and the running binary's version, so a
// consumer can tell which build it is talking to.
func handleHealth(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(healthResponse{Status: "ok", Version: version.Version})
}

func (s *Server) handleCheck(w http.ResponseWriter, r *http.Request) {
	req := &apiv1.CheckRequest{}
	if err := readMessage(r, req); err != nil {
		slog.Error("http: check decode failed", "error", err)
		writeError(w, status.New(codes.InvalidArgument, err.Error()))
		return
	}

	result, err := s.service.Check(r.Context(), req.GetUrl())
	if err != nil {
		st := toStatus(err)
		slog.Error("http: check failed", "url", req.GetUrl(), "code", st.Code(), "error", err)
		writeError(w, st)
		return
	}

	// 200: the history entry the check leaves is a record of the call, not a
	// business object the caller asked to create.
	writeMessage(w, http.StatusOK, &apiv1.CheckResponse{
		Url:        result.URL,
		Normalized: result.Normalized,
		Flagged:    result.Flagged,
		Reason:     result.Reason,
	})
}

func (s *Server) handleRecent(w http.ResponseWriter, r *http.Request) {
	// A GET has no body: the request message is filled from the query.
	req := &apiv1.RecentChecksRequest{}
	if v := r.URL.Query().Get("limit"); v != "" {
		n, err := strconv.ParseInt(v, 10, 32)
		if err != nil {
			writeError(w, status.New(codes.InvalidArgument, "limit: "+err.Error()))
			return
		}
		req.Limit = int32(n)
	}
	limit := int(req.GetLimit())
	if limit <= 0 {
		limit = 20
	}

	entries, err := s.service.RecentChecks(r.Context(), limit)
	if err != nil {
		st := toStatus(err)
		slog.Error("http: recent failed", "code", st.Code(), "error", err)
		writeError(w, st)
		return
	}

	out := make([]*apiv1.HistoryEntry, len(entries))
	for i, e := range entries {
		out[i] = &apiv1.HistoryEntry{
			Normalized:    e.Normalized,
			Flagged:       e.Flagged,
			Reason:        e.Reason,
			CheckedAtUnix: e.CheckedAt.Unix(),
		}
	}
	writeMessage(w, http.StatusOK, &apiv1.RecentChecksResponse{Entries: out})
}

// toStatus translates a domain error into the grpc status the contract answers
// with; the HTTP status follows from its code.
func toStatus(err error) *status.Status {
	switch {
	case errors.Is(err, services.ErrInvalidURL):
		return status.New(codes.InvalidArgument, err.Error())
	case errors.Is(err, interfaces.ErrUnavailable):
		return status.New(codes.Unavailable, err.Error())
	default:
		return status.New(codes.Internal, err.Error())
	}
}
