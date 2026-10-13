---
name: plateau-http-service--file-api-http-errors
description: internal/api/http/errors.go of the plateau-http-service plateau
whenToUse: when creating or editing internal/api/http/errors.go, or adding an error code the HTTP adapter answers with
domain: skill
type: template
plateau: plateau-http-service
version: 20261012000000
tags:
  - skill/template/file
  - plateau/plateau-http-service
created_by:
  - "[[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
---

# Goal
Answer every error with one body shape — `google.rpc.Status` — and a status that follows from its code.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/errors.go.create.md|errors.go]]

# Core Principles
- Apply ONE plateau template per file.
- The codes are those of `google.rpc.Code`, written by hand: this plateau has no protobuf dependency.

# Implementation
```go
// Skill: file-api-http-errors
// Plateau: plateau-http-service
// Version: 20261012000000

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
Verified against this plateau's own `examples/internal/api/http/errors.go` — `POST /v1/links/check {"url":"not-a-url"}` → `400 {"code":3,"message":"services: invalid url","details":[]}`.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/errors.go.create.md|errors.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- A new code is a constant with its `google.rpc.Code` number and a row of `httpStatus` from the canonical gRPC-to-HTTP table, added together.
- Every error is written through `writeError` — never `http.Error` or an ad hoc map.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/errors.go.create.md#MUST|errors.go]]

# Check list
- [ ] Every `code…` constant has a row in `httpStatus`.
- [ ] No `http.Error` call and no `{"error": …}` body in the package.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/errors.go.create.md|errors.go]]

# Unittest TestCases
- [ ] WHEN a request is invalid for the domain THEN it answers 400 with `code` 3
- [ ] WHEN a request body cannot be parsed THEN it answers 400 with `code` 3

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/errors.go.create.md|errors.go]]
