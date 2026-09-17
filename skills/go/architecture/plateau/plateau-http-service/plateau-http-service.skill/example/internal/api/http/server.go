// Package http is the inbound HTTP adapter: it decodes requests, calls the
// domain service, and encodes the result back as JSON.
package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"

	"github.com/example/linkcheck-service/internal/domain/services"
)

type Server struct {
	service *services.LinkCheckService
}

func New(service *services.LinkCheckService) *Server {
	return &Server{service: service}
}

func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /health", handleHealth)
	mux.HandleFunc("POST /v1/links/check", s.handleCheck)
	return mux
}

func handleHealth(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write([]byte("ok"))
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
		writeError(w, http.StatusBadRequest, err)
		return
	}

	result, err := s.service.Check(r.Context(), req.URL)
	if err != nil {
		code := http.StatusInternalServerError
		if errors.Is(err, services.ErrInvalidURL) {
			code = http.StatusBadRequest
		}
		slog.Error("http: check failed", "url", req.URL, "status", code, "error", err)
		writeError(w, code, err)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(checkResponse{URL: result.URL, Normalized: result.Normalized})
}

func writeError(w http.ResponseWriter, code int, err error) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	_ = json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
}
