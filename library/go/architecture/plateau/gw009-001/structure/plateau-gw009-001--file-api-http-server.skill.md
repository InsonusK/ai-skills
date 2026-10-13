---
name: plateau-gw009-001--file-api-http-server
description: internal/api/http/server.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when creating or editing internal/api/http/server.go, or adding a new HTTP route
domain: skill
type: template
plateau: plateau-gw009-001
version: 20261012000000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
registry:
  - "[[library/go/architecture/registry/internal-api-http-server-go.md|internal-api-http-server-go]]"
---

# Goal
Translate HTTP requests into calls on `LinkCheckService` and translate results (including the reputation verdict and the recorded history) back into JSON.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[library/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Holds the domain service's concrete type, shared with every other inbound adapter.
- Every field the domain result carries is reachable through this response — nothing computed is silently dropped.
- The history-read route returns a bounded, caller-limitable page — never an unbounded dump of the store.

# Implementation
```go
// Skill: file-api-http-server
// Plateau: plateau-gw009-001
// Version: 20261012000000

package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"
	"strconv"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "{module-path}/gen/api/linkcheck"
	"{module-path}/internal/domain/interfaces"
	"{module-path}/internal/domain/services"
	"{module-path}/internal/version"
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
```
Verified against this plateau's own `examples/internal/api/http/server.go` by `internal/api/http/features/http_api.feature`, run through `Server.Handler()` (`make test-kind-unit` green): the served routes equal the contract's operations; `POST /v1/links/check {"url":"https://Example.com/Path"}` → `200` with `"normalized":"https://example.com/Path"`; a request with an unknown field → `200`; `{"url":"not-a-url"}` and an unparseable body → `400` with `"code":3`; a flagged URL → `200` with `"flagged":true` and the reason; an unavailable reputation service → `503` with `"code":14`; `GET /v1/links/recent?limit=1` after two checks → the newer entry only, inside `{"entries":[…]}`; `limit=two` → `400`; `GET /health` → `200` with the binary's version. The stores and the reputation service are stubs in this feature — the live path through PostgreSQL and Redis is this plateau's `file-cmd-service-main` verification, made before the adapter moved onto the generated messages.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[library/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` holds the domain service's concrete type — never construct a second instance.
- Every handler translates a known domain sentinel error to its grpc code, and the HTTP status follows from the code by the canonical table; anything else is `INTERNAL`, 500.
- Every route of the contract is an entry of `contractRoutes()`; `/health` and the docs routes are registered outside it.
- Every handler reads into, and answers with, the generated message of its `rpc` through `readMessage`/`writeMessage` — never a struct of its own; a route's pattern equals its `rpc`'s `google.api.http` annotation.
- A handler answers success with 201 when it created a business object, 202 when the work is only accepted, 200 otherwise.
- Every field the port adds to the domain result appears in the contract's `CheckResponse`; the port's unavailable sentinel maps to `UNAVAILABLE`, answered 503.
- `GET /v1/links/recent` accepts an optional, bounded `limit` query parameter (default 20) — never an unbounded result set.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md#MUST|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]
- [[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[library/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]

# Check list
- [ ] `GET /health` returns `200` with `{"status":"ok","version":<version.Version>}` unconditionally.
- [ ] Every handler's error path writes a `google.rpc.Status` body — `{"code": …, "message": "…", "details": []}`.
- [ ] `Handler()` holds no `mux.HandleFunc` line for a `/v1/` route.
- [ ] The response carries `flagged`/`reason`; the unavailable sentinel is answered 503 with `code` 14.
- [ ] `GET /v1/links/recent` returns `{"entries": […]}` most-recent-first and respects `limit`; a `limit` that is not a number is answered 400.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[library/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Unittest TestCases
- [ ] WHEN `GET /health` is called with `version.Version` set to `1.2.3` THEN it returns `200` and a JSON body whose `version` is `1.2.3`
- [ ] WHEN `POST /v1/links/check` is given a well-formed, unflagged URL THEN it returns `200` with `"flagged":false`
- [ ] WHEN `POST /v1/links/check` is given a URL the reputation service flags THEN it returns `200` with `"flagged":true` and the reason
- [ ] WHEN `POST /v1/links/check` is given an invalid URL THEN it returns `400` with `code` 3
- [ ] WHEN the operations of the contract are compared THEN `ContractRoutes()` and the embedded spec have exactly the same set
- [ ] WHEN the reputation service is unreachable THEN it returns `503` with `code` 14
- [ ] WHEN `GET /v1/links/recent` is called after 2 checks THEN it returns exactly those 2 entries, most-recent-first

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[library/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
- [[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[library/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
