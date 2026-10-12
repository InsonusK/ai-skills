---
name: plateau-integrated-service--file-api-http-protojson
description: internal/api/http/protojson.go of the plateau-integrated-service plateau
whenToUse: when creating or editing internal/api/http/protojson.go, or changing how the HTTP adapter reads a request, writes a response, or maps an error to a status
domain: skill
type: template
plateau: plateau-integrated-service
version: 20261012000000
tags:
  - skill/template/file
  - plateau/plateau-integrated-service
created_by:
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
---

# Goal
Read every request and write every response and error of the HTTP form through the contract's generated messages.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/protojson.go.create.md|protojson.go]]

# Core Principles
- Apply ONE plateau template per file.
- The `protojson` options are fixed once for the package: a handler never calls `protojson` or `encoding/json` itself.

# Implementation
```go
// Skill: file-api-http-protojson
// Plateau: plateau-integrated-service
// Version: 20261012000000

package http

import (
	"io"
	"net/http"

	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
	"google.golang.org/protobuf/encoding/protojson"
	"google.golang.org/protobuf/proto"
)

// Standard proto-JSON: a request is read with either form of a field name and
// its unknown fields are ignored; a response names every field in lowerCamelCase,
// also one that holds its default value.
var (
	unmarshal = protojson.UnmarshalOptions{DiscardUnknown: true}
	marshal   = protojson.MarshalOptions{EmitUnpopulated: true}
)

// readMessage decodes the request body into the contract's message.
func readMessage(r *http.Request, message proto.Message) error {
	body, err := io.ReadAll(r.Body)
	if err != nil {
		return err
	}
	return unmarshal.Unmarshal(body, message)
}

// writeMessage encodes the contract's message as the response body.
func writeMessage(w http.ResponseWriter, httpStatus int, message proto.Message) {
	body, err := marshal.Marshal(message)
	if err != nil {
		writeError(w, status.New(codes.Internal, err.Error()))
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(httpStatus)
	_, _ = w.Write(body)
}

// httpStatuses is the canonical gRPC-to-HTTP mapping.
var httpStatuses = map[codes.Code]int{
	codes.InvalidArgument:    http.StatusBadRequest,
	codes.FailedPrecondition: http.StatusBadRequest,
	codes.OutOfRange:         http.StatusBadRequest,
	codes.Unauthenticated:    http.StatusUnauthorized,
	codes.PermissionDenied:   http.StatusForbidden,
	codes.NotFound:           http.StatusNotFound,
	codes.AlreadyExists:      http.StatusConflict,
	codes.Aborted:            http.StatusConflict,
	codes.ResourceExhausted:  http.StatusTooManyRequests,
	codes.Canceled:           499,
	codes.Internal:           http.StatusInternalServerError,
	codes.Unknown:            http.StatusInternalServerError,
	codes.DataLoss:           http.StatusInternalServerError,
	codes.Unimplemented:      http.StatusNotImplemented,
	codes.Unavailable:        http.StatusServiceUnavailable,
	codes.DeadlineExceeded:   http.StatusGatewayTimeout,
}

// writeError answers with google.rpc.Status as the body and the HTTP status its
// code maps to.
func writeError(w http.ResponseWriter, st *status.Status) {
	httpStatus, ok := httpStatuses[st.Code()]
	if !ok {
		httpStatus = http.StatusInternalServerError
	}
	body, err := marshal.Marshal(st.Proto())
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(httpStatus)
	_, _ = w.Write(body)
}
```
Verified against this plateau's own `examples/internal/api/http/protojson.go` — `POST /v1/links/check {"url":"not-a-url"}` → `400 {"code":3,"message":"services: invalid url","details":[]}`; a request with an unknown field → `200`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/protojson.go.create.md|protojson.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `DiscardUnknown: true` on reading and `EmitUnpopulated: true` on writing stay as they are.
- `httpStatuses` equals the canonical gRPC-to-HTTP table of `api-develop` — a handler changes the code it answers with, never a row.
- The package holds no `errors.go` and no request or response struct of the contract.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/protojson.go.create.md#MUST|protojson.go]]

# Check list
- [ ] No handler calls `protojson`, `json.NewDecoder`, or `json.NewEncoder` for a contract message.
- [ ] Every code the canonical table names has a row in `httpStatuses`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/protojson.go.create.md|protojson.go]]

# Unittest TestCases
- [ ] WHEN a request carries a field the contract does not know THEN it succeeds
- [ ] WHEN a request body cannot be parsed THEN it answers 400 with `code` 3
- [ ] WHEN a response field holds its default value THEN the body still names it

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/api/http/protojson.go.create.md|protojson.go]]
