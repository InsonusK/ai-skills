---
name: plateau-dual-api-service--file-api-http-server
description: internal/api/http/server.go of the plateau-dual-api-service plateau
whenToUse: when creating or editing internal/api/http/server.go, or adding a new HTTP route
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20261012000000
tags:
  - skill/template/file
  - plateau/plateau-dual-api-service
created_by:
  - "[[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Goal
Translate HTTP requests into calls on `LinkCheckService` and translate results back into JSON — the HTTP form of the service's contract.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Core Principles
- Apply ONE plateau template per file.
- Holds the domain service's concrete type, shared with every other inbound adapter.

# Implementation
```go
// Skill: file-api-http-server
// Plateau: plateau-dual-api-service
// Version: 20261012000000

package http

import (
	"encoding/json"
	"errors"
	"log/slog"
	"net/http"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "{module-path}/gen/api/linkcheck"
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

	// 200: the method created nothing and its work is done.
	writeMessage(w, http.StatusOK, &apiv1.CheckResponse{Url: result.URL, Normalized: result.Normalized})
}

// toStatus translates a domain error into the grpc status the contract answers
// with; the HTTP status follows from its code.
func toStatus(err error) *status.Status {
	if errors.Is(err, services.ErrInvalidURL) {
		return status.New(codes.InvalidArgument, err.Error())
	}
	return status.New(codes.Internal, err.Error())
}
```
Verified against this plateau's own `examples/internal/api/http/server.go` by `internal/api/http/features/http_api.feature`, run through `Server.Handler()` (`make test-kind-unit` green): the served routes equal the contract's operations; `POST /v1/links/check {"url":"https://Example.com/Path"}` → `200` with `"normalized":"https://example.com/Path"`; a request with an unknown field → `200`; `{"url":"not-a-url"}` and an unparseable body → `400` with `"code":3`; `GET /health` → `200` with the binary's version.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Server` holds the domain service's concrete type — never construct a second instance.
- Every handler translates a known domain sentinel error to its grpc code, and the HTTP status follows from the code by the canonical table; anything else is `INTERNAL`, 500.
- Every route of the contract is an entry of `contractRoutes()`; `/health` and the docs routes are registered outside it.
- Every handler reads into, and answers with, the generated message of its `rpc` through `readMessage`/`writeMessage` — never a struct of its own; a route's pattern equals its `rpc`'s `google.api.http` annotation.
- A handler answers success with 201 when it created a business object, 202 when the work is only accepted, 200 otherwise.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md#MUST|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md#MUST|server.go]]

# Check list
- [ ] `GET /health` returns `200` with `{"status":"ok","version":<version.Version>}` unconditionally.
- [ ] Every handler's error path writes a `google.rpc.Status` body — `{"code": …, "message": "…", "details": []}`.
- [ ] `Handler()` holds no `mux.HandleFunc` line for a `/v1/` route.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]

# Unittest TestCases
- [ ] WHEN `GET /health` is called with `version.Version` set to `1.2.3` THEN it returns `200` and a JSON body whose `version` is `1.2.3`
- [ ] WHEN `POST /v1/links/check` is given a well-formed URL THEN it returns `200` with the normalized form
- [ ] WHEN `POST /v1/links/check` is given an invalid URL THEN it returns `400` with `code` 3
- [ ] WHEN the operations of the contract are compared THEN `ContractRoutes()` and the embedded spec have exactly the same set

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|server.go]]
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/server.go.extend.md|server.go]]
