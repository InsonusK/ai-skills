---
description: Proto-JSON in and out and the canonical error mapping of the HTTP adapter — replaces errors.go
project_name: internal/api/http
name: protojson
element_kind: functions
change_kind: create
tags:
  - solution/grpc-api
  - element/internal-api-http-protojson-go
---

# Goals
- Read every request and write every response and error of the HTTP form through the contract's generated messages.

# Core Principles
- The options are fixed once for the package: a handler never calls `protojson` or `encoding/json` itself.

# Implementation changes
`errors.go` is deleted in the same change: `writeError` now takes a `*status.Status`, and the code-to-status table is complete.
```go
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

# Rule changes

## MUST
- Keep `DiscardUnknown: true` on reading and `EmitUnpopulated: true` on writing.
  - Violation: `protojson.Unmarshal(body, message)` with default options; `protojson.Marshal` without `EmitUnpopulated`.
  - Risk: a client built against a newer minor is refused by an older server; a response loses every field that holds its default value, which the baseline always wrote — a change for existing clients.
  - Fix: the two package variables above, used by `readMessage` and `writeMessage` only.
- Keep `httpStatuses` equal to the canonical table of [[library/common-workflow/develop/api-develop.skill/api-develop.skill.md|api-develop]].
  - Violation: `codes.Unavailable: http.StatusBadGateway`.
  - Risk: the HTTP status and the grpc status of one outcome disagree.
  - Fix: change the code a handler answers with, never a row of the table.

# Check list
- [ ] `internal/api/http/` holds no `errors.go`, no `encoding/json` import outside `server.go`'s health handler and `docs.go`, and no request or response struct of the contract.
- [ ] Every code of `google.golang.org/grpc/codes` the canonical table names has a row.
