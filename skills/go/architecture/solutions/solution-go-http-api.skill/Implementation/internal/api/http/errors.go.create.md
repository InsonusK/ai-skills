---
description: The error body and the code-to-status mapping of the HTTP adapter
project_name: internal/api/http
name: errors
element_kind: functions
change_kind: create
tags:
  - solution/go-http-api
  - element/internal-api-http-errors-go
---

# Goals
- Answer every error with one body shape and a status that follows from its code.

# Core Principles
- The body is `google.rpc.Status` and the codes are those of `google.rpc.Code`, written by hand because this service has no protobuf dependency yet.

# Implementation changes
```go
package http

import (
	"encoding/json"
	"net/http"
)

// The grpc status codes this adapter answers with. The numbers are those of
// google.rpc.Code, so the error body reads the same as in a service that has a
// grpc contract.
const (
	codeInvalidArgument = 3
	codeInternal        = 13
)

// httpStatus is the canonical gRPC-to-HTTP mapping for the codes above; a code
// not listed is a server fault.
var httpStatus = map[int]int{
	codeInvalidArgument: http.StatusBadRequest,
	codeInternal:        http.StatusInternalServerError,
}

// statusBody is google.rpc.Status in proto-JSON.
type statusBody struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
	Details []any  `json:"details"`
}

func writeError(w http.ResponseWriter, code int, err error) {
	status, ok := httpStatus[code]
	if !ok {
		status = http.StatusInternalServerError
	}
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(statusBody{Code: code, Message: err.Error(), Details: []any{}})
}
```

# Rule changes

## MUST
- Add a code this adapter starts to answer with as a constant with its `google.rpc.Code` number and as a row of `httpStatus` taken from the canonical table of [[skills/common-workflow/develop/api-develop.skill/api-develop.skill.md|api-develop]].
  - Violation: `writeError(w, 404, err)`; a `codeNotFound` constant with no `httpStatus` row.
  - Risk: an HTTP status passed as a code writes `"code": 404` into a body whose code is a grpc code; a code without a row answers 500.
  - Fix: constant and row together, in the change that adds the handler using them.
- Write every error through `writeError` — never `http.Error` or an ad hoc map.
  - Risk: two outcomes with one status cannot be told apart, and a client needs a second parser.
  - Fix: one call site shape, `writeError(w, code, err)`.

# Check list
- [ ] Every `code…` constant has a row in `httpStatus`.
- [ ] No `http.Error` call and no `map[string]string{"error": …}` in the package.
