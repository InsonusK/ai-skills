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

# Core Principles
- Holds the domain service's concrete type, not an interface — there is only ever one domain-service instance, shared by every inbound adapter.

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
)

type Server struct {
	service *services.{Service}
}

func New(service *services.{Service}) *Server {
	return &Server{service: service}
}

func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /health", handleHealth)
	return mux
}

func handleHealth(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
	_, _ = w.Write([]byte("ok"))
}

func writeError(w http.ResponseWriter, code int, err error) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	_ = json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
}
```

This catalog's own runnable examples add the concrete route(s) calling `{Service}`'s real method (e.g. `POST /v1/links/check` calling `LinkCheckService.Check`) — see `plateau-http-service`'s `example/`.

# Rule changes

## MUST
- `Server` must hold the domain service's concrete type — never construct a second instance of it.
  - Risk: a second, independently-constructed domain-service instance can drift from the one other adapters use (e.g. wired with different dependencies), so behavior differs by which entry point handled the call.
  - Fix: accept the domain service as a constructor argument; `main.go` constructs it once and passes the same pointer to every adapter.
- Every handler must translate a known domain sentinel error to a specific HTTP status and default every other error to `500`.
  - Risk: returning `500` for every error (or leaking the raw Go error string as the only signal) gives callers no way to distinguish a client mistake from a server fault.
  - Fix: `errors.Is` against the domain's sentinel errors first; fall back to `500` only for the unmatched case.

# Check list
- [ ] `GET /health` returns `200` unconditionally once the process has finished wiring.
- [ ] Every handler's error path writes a JSON `{"error": "..."}` body.

# Unittest TestCases
- [ ] WHEN `GET /health` is called THEN it returns `200`
