---
description: Surface the port's result in the HTTP response, and map the port's unavailable sentinel to UNAVAILABLE
project_name: internal/api/http
name: Server
element_kind: struct
change_kind: extend
tags:
  - solution/external-integration
  - element/internal-api-http-server-go
---

# Implementation changes
With [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] applied — the fields are added to the proto message, `make proto-gen` regenerates, and the handler copies them:
```go
writeMessage(w, http.StatusOK, &apiv1.CheckResponse{
	Url:        result.URL,
	Normalized: result.Normalized,
	// ... plus whatever field(s) this solution's port adds to {Result} — e.g.:
	Flagged: result.Flagged,
	Reason:  result.Reason,
})

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

Without it — the fields are added to the handler's own struct and to `openapi.json`, and the code to `errors.go` with its row (`codeUnavailable = 14` → `http.StatusServiceUnavailable`):
```go
type checkResponse struct {
	URL        string `json:"url"`
	Normalized string `json:"normalized"`
	Flagged    bool   `json:"flagged"`
	Reason     string `json:"reason"`
}
```

# Rule changes

## MUST
- Every new field this solution's port adds to `{Result}` must appear in the response of the contract — never left silently unreachable through the HTTP transport.
  - Risk: a caller integrating over HTTP has no way to observe data the domain service already computed, defeating the point of adding the port.
  - Fix: add the field to the contract (the proto message, or the struct and `openapi.json`), bump the contract's minor version, and copy it in the handler — in the change that extends the domain service.
- The port's own unavailable sentinel (`interfaces.ErrUnavailable`) must map to `UNAVAILABLE`, answered 503 — distinct from `INVALID_ARGUMENT`, 400.
  - Violation: `http.StatusBadGateway`; the sentinel left to the `INTERNAL` default.
  - Risk: 502 is not in the canonical gRPC-to-HTTP table, so the HTTP and the grpc answer of one outcome disagree; 500 hides from the caller that retrying makes sense.
  - Fix: one `case` per distinguishable sentinel error, as shown.

# Check list
- [ ] Every field `{Result}` gained is present in the contract's response and in the HTTP body.
- [ ] The port's unavailable sentinel is answered 503 with `code` 14, not 502, 500, or 400.
