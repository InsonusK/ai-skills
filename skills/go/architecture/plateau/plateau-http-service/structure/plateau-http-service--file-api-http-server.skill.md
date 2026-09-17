---
name: plateau-http-service--file-api-http-server
description: internal/api/http/server.go of the plateau-http-service plateau
whenToUse: when creating or editing internal/api/http/server.go, or adding a new HTTP route
domain: skill
type: template
plateau: plateau-http-service
version: 20260917000000
tags:
  - skill/template/file
  - plateau/plateau-http-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
---

# Goal
Translate HTTP requests into calls on `LinkCheckService` and translate results back into JSON.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Holds the domain service's concrete type, shared with every other inbound adapter.

# Implementation
```go
// Skill: file-api-http-server
// Plateau: plateau-http-service
// Version: 20260917000000

package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"

	"{module-path}/internal/domain/services"
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
```
Verified against this plateau's own `example/internal/api/http/server.go` — smoke-tested: `GET /health` → `200`; `POST /v1/links/check {"url":"HTTPS://Example.com/Foo"}` → `200 {"url":"...","normalized":"https://example.com/Foo"}`; `POST /v1/links/check {"url":"not-a-url"}` → `400`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` holds the domain service's concrete type — never construct a second instance.
- Every handler translates a known domain sentinel error to a specific HTTP status; anything else defaults to `500`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md#MUST|server.go]]

# Check list
- [ ] `GET /health` returns `200` unconditionally.
- [ ] Every handler's error path writes a JSON `{"error": "..."}` body.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]

# Unittest TestCases
- [ ] WHEN `GET /health` is called THEN it returns `200`
- [ ] WHEN `POST /v1/links/check` is given a well-formed URL THEN it returns `200` with the normalized form
- [ ] WHEN `POST /v1/links/check` is given an invalid URL THEN it returns `400`

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
