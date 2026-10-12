---
description: The HTTP handlers read and write the contract's generated messages
project_name: internal/api/http
name: Server
element_kind: struct
change_kind: extend
tags:
  - solution/grpc-api
  - element/internal-api-http-server-go
---

# Goals
- Make the proto file the one description of every request and response shape, for both transports.

# Implementation changes
`Server`, `Options`, `contractRoutes()`, `ContractRoutes()`, `Handler()`, and the health handler stay as [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/server.go.create.md|solution-go-http-api]] wrote them. Every handler of a contract route changes — its own request and response structs are deleted:
```go
import (
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	apiv1 "{module-path}/gen/api/{contract-name}"
)

func (s *Server) handle{Method}(w http.ResponseWriter, r *http.Request) {
	req := &apiv1.{Method}Request{}
	if err := readMessage(r, req); err != nil {
		slog.Error("http: {method} decode failed", "error", err)
		writeError(w, status.New(codes.InvalidArgument, err.Error()))
		return
	}

	result, err := s.service.{Method}(r.Context(), req.GetInput())
	if err != nil {
		st := toStatus(err)
		slog.Error("http: {method} failed", "code", st.Code(), "error", err)
		writeError(w, st)
		return
	}

	// 200: the method created nothing and its work is done.
	writeMessage(w, http.StatusOK, &apiv1.{Method}Response{ /* fields of result */ })
}

// toStatus translates a domain error into the grpc status the contract answers
// with; the HTTP status follows from its code.
func toStatus(err error) *status.Status {
	if errors.Is(err, services.Err{Sentinel}) {
		return status.New(codes.InvalidArgument, err.Error())
	}
	return status.New(codes.Internal, err.Error())
}
```
The comment of `contractRoutes()` now reads "generated from the proto file and these routes are written by hand".

A `GET` route has no body: its handler fills the request message from `r.URL.Query()` and answers `INVALID_ARGUMENT` for a value it cannot parse — see `plateau-persistent-service`'s `handleRecent`.

# Rule changes

## MUST
- Every handler of a contract route must read into, and answer with, the generated message of its `rpc` — never a struct of its own.
  - Violation: `type checkResponse struct{…}` kept beside `apiv1.CheckResponse`.
  - Risk: a field added to the proto file reaches gRPC callers and the spec, and not the HTTP response.
  - Fix: delete the struct; build the generated message from the domain result.
- A route's pattern in `contractRoutes()` must equal its `rpc`'s `google.api.http` annotation.
  - Risk: the route and the spec are typed twice; the operations scenario fails when they differ, and only if the route is in the list.
  - Fix: change the annotation and the pattern in one commit, run `make proto-gen`, update the scenario's table.

# Check list
- [ ] `server.go` declares no struct but `Options`, `Server`, `contractRoute`, and `healthResponse`.
- [ ] `toStatus` in `internal/api/http` and in `internal/api/grpc` map every domain sentinel to the same code.
