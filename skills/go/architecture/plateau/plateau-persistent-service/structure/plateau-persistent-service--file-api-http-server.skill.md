---
name: plateau-persistent-service--file-api-http-server
description: internal/api/http/server.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/api/http/server.go, or adding a new HTTP route
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
registry:
  - "[[../registry/internal-api-http-server-go.md|internal-api-http-server-go]]"
---

# Goal
Translate HTTP requests into calls on `LinkCheckService` and translate results (including the reputation verdict and the recorded history) back into JSON.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Holds the domain service's concrete type, shared with every other inbound adapter.
- Every field the domain result carries is reachable through this response — nothing computed is silently dropped.
- The history-read route returns a bounded, caller-limitable page — never an unbounded dump of the store.

# Implementation
```go
// Skill: file-api-http-server
// Plateau: plateau-persistent-service
// Version: 20260917040000

package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"
	"strconv"
	"time"

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
	mux.HandleFunc("GET /v1/links/recent", s.handleRecent)
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
		slog.Error("http: check decode failed", "error", err)
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

type historyEntry struct {
	Normalized string    `json:"normalized"`
	Flagged    bool      `json:"flagged"`
	Reason     string    `json:"reason,omitempty"`
	CheckedAt  time.Time `json:"checked_at"`
}

func (s *Server) handleRecent(w http.ResponseWriter, r *http.Request) {
	limit := 20
	if v := r.URL.Query().Get("limit"); v != "" {
		if n, err := strconv.Atoi(v); err == nil && n > 0 {
			limit = n
		}
	}

	entries, err := s.service.RecentChecks(r.Context(), limit)
	if err != nil {
		slog.Error("http: recent failed", "error", err)
		writeError(w, http.StatusInternalServerError, err)
		return
	}

	out := make([]historyEntry, len(entries))
	for i, e := range entries {
		out[i] = historyEntry{Normalized: e.Normalized, Flagged: e.Flagged, Reason: e.Reason, CheckedAt: e.CheckedAt}
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(out)
}

func writeError(w http.ResponseWriter, code int, err error) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	_ = json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
}
```
Verified against this plateau's own `example/internal/api/http/server.go` — smoke-tested: two `POST /v1/links/check` calls followed by `GET /v1/links/recent?limit=10` returned both entries, most-recent-first, with correct `checked_at`; a direct `psql` query against `link_checks` confirmed the same two rows; **the service was then killed and restarted, and `GET /v1/links/recent` (no new checks made) returned the identical two entries** — proving durable persistence across a process restart, not just within one process lifetime.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` holds the domain service's concrete type — never construct a second instance.
- Every handler translates a known domain sentinel error to a specific HTTP status; anything else defaults to `500`.
- Every field the port adds to the domain result appears in `checkResponse`; the port's unavailable sentinel maps to `502`.
- `GET /v1/links/recent` accepts an optional, bounded `limit` query parameter (default 20) — never an unbounded result set.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md#MUST|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]

# Check list
- [ ] `GET /health` returns `200` unconditionally.
- [ ] Every handler's error path writes a JSON `{"error": "..."}` body.
- [ ] `checkResponse` carries `flagged`/`reason`; the unavailable sentinel maps to `502`.
- [ ] `GET /v1/links/recent` returns entries most-recent-first and respects `limit`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Unittest TestCases
- [ ] WHEN `GET /health` is called THEN it returns `200`
- [ ] WHEN `POST /v1/links/check` is given a well-formed, unflagged URL THEN it returns `200` with `"flagged":false`
- [ ] WHEN `POST /v1/links/check` is given a URL the reputation service flags THEN it returns `200` with `"flagged":true` and the reason
- [ ] WHEN `POST /v1/links/check` is given an invalid URL THEN it returns `400`
- [ ] WHEN the reputation service is unreachable THEN it returns `502`
- [ ] WHEN `GET /v1/links/recent` is called after 2 checks THEN it returns exactly those 2 entries, most-recent-first

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
