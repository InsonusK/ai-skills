---
description: Server struct — thin HTTP adapter over the domain service
project_name: internal/api/http
name: Server
element_kind: struct
change_kind: create
tags:
  - solution/go-http-api
  - element/internal-api-http-server-go
---

# Goals
- Translate HTTP requests into calls on the domain service and translate its results back into HTTP responses.
- Keep the contract's routes in one list a test can read.

# Core Principles
- Holds the domain service's concrete type, not an interface — there is only ever one domain-service instance, shared by every inbound adapter.
- Routes stay mounted at `/`: `Options.BasePath` goes only into URLs handed to a client.

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------- | --------- |
| the HTTP adapter | `Server` | `Server` | `server.go` | `server.go` |

# Implementation changes
```go
// Package http is the inbound HTTP adapter: it decodes requests, calls the
// domain service, and encodes the result back as JSON.
package http

import (
	"encoding/json"
	"net/http"

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
	service *services.{Service}
	options Options
}

func New(service *services.{Service}, options Options) *Server {
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
		// {"POST /v1/{resource}", s.handle{Method}},
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
```

A handler, as this catalog's own runnable example writes it (`POST /v1/links/check` calling `LinkCheckService.Check` — see `plateau-http-service`'s `examples/`):
```go
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
```

# Rule changes

## MUST
- `Server` must hold the domain service's concrete type — never construct a second instance of it.
  - Risk: a second, independently-constructed domain-service instance can drift from the one other adapters use (e.g. wired with different dependencies), so behavior differs by which entry point handled the call.
  - Fix: accept the domain service as a constructor argument; `main.go` constructs it once and passes the same pointer to every adapter.
- Every route of the contract must be an entry of `contractRoutes()`; `/health` and the docs routes are registered outside it.
  - Violation: `mux.HandleFunc("POST /v1/…", …)` written in `Handler()` beside the loop.
  - Risk: a route outside the list is invisible to the operations scenario, so the spec can describe an endpoint that answers 404 while every test passes.
  - Fix: add the route to the list; `Handler()` registers the list in one loop.
- Every handler must translate a known domain sentinel error to its grpc code and default every other error to `codeInternal`.
  - Risk: answering 500 for every error gives callers no way to distinguish a client mistake from a server fault.
  - Fix: `errors.Is` against the domain's sentinel errors first; fall back to `codeInternal` only for the unmatched case.
- Every request and response struct must name its JSON keys in lowerCamelCase, write every field (no `omitempty`), and leave unknown request fields ignored.
  - Violation: `json:"checked_at"`; `json:"reason,omitempty"`; `decoder.DisallowUnknownFields()`.
  - Risk: the JSON differs from what `protojson` writes for the same fields, so composing [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] changes what every existing client reads.
  - Fix: lowerCamelCase tags without `omitempty`; a plain `json.NewDecoder(r.Body).Decode`.
- Every handler must answer success with 201 when it created a business object, 202 when the work is only accepted, and 200 otherwise, with a comment on the line saying which case it is.
  - Risk: a caller treats queued work as finished.
  - Fix: `w.WriteHeader` with the status before the body is encoded; 200 needs no call.

# Check list
- [ ] `GET /health` returns `200` with `{"status":"ok","version":<version.Version>}` unconditionally once the process has finished wiring.
- [ ] `Handler()` holds no `mux.HandleFunc` line for a `/v{n}/` route.
- [ ] No JSON tag in the package holds `_` or `omitempty`.

# Unittest TestCases
- [ ] WHEN `GET /health` is called with `version.Version` set to `1.2.3` THEN it returns `200` and a JSON body whose `version` is `1.2.3`
