---
name: plateau-cached-service--file-api-http-server
description: internal/api/http/server.go of the plateau-cached-service plateau
whenToUse: when creating or editing internal/api/http/server.go, or adding a new HTTP route
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/file
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Goal
Translate HTTP requests into calls on `LinkCheckService` and translate results (including the reputation verdict) back into JSON.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Holds the domain service's concrete type, shared with every other inbound adapter.
- Every field the domain result carries is reachable through this response — nothing computed is silently dropped.

# Implementation
```go
// Skill: file-api-http-server
// Plateau: plateau-cached-service
// Version: 20260917030000

package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"

	"{module-path}/internal/domain/interfaces"
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
	Flagged    bool   `json:"flagged"`
	Reason     string `json:"reason,omitempty"`
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
		switch {
		case errors.Is(err, services.ErrInvalidURL):
			code = http.StatusBadRequest
		case errors.Is(err, interfaces.ErrUnavailable):
			code = http.StatusBadGateway
		}
		slog.Error("http: check failed", "url", req.URL, "status", code, "error", err)
		writeError(w, code, err)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(checkResponse{
		URL:        result.URL,
		Normalized: result.Normalized,
		Flagged:    result.Flagged,
		Reason:     result.Reason,
	})
}

func writeError(w http.ResponseWriter, code int, err error) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	_ = json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
}
```
Verified against this plateau's own `example/internal/api/http/server.go` — smoke-tested: `POST /v1/links/check {"url":"https://good.example.com"}` → `200 {"flagged":false,...}`; a URL the fake reputation server flags → `200 {"flagged":true,"reason":"..."}`; reputation service stopped → `502`; invalid URL → `400`; `GET /health` → `200`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` holds the domain service's concrete type — never construct a second instance.
- Every handler translates a known domain sentinel error to a specific HTTP status; anything else defaults to `500`.
- Every field the port adds to the domain result appears in `checkResponse`; the port's unavailable sentinel maps to `502`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md#MUST|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]

# Check list
- [ ] `GET /health` returns `200` unconditionally.
- [ ] Every handler's error path writes a JSON `{"error": "..."}` body.
- [ ] `checkResponse` carries `flagged`/`reason`; the unavailable sentinel maps to `502`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Unittest TestCases
- [ ] WHEN `GET /health` is called THEN it returns `200`
- [ ] WHEN `POST /v1/links/check` is given a well-formed, unflagged URL THEN it returns `200` with `"flagged":false`
- [ ] WHEN `POST /v1/links/check` is given a URL the reputation service flags THEN it returns `200` with `"flagged":true` and the reason
- [ ] WHEN `POST /v1/links/check` is given an invalid URL THEN it returns `400`
- [ ] WHEN the reputation service is unreachable THEN it returns `502`

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
