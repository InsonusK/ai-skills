---
description: Surface the port's result in the HTTP response, and map the port's unavailable sentinel to a status code
project_name: internal/api/http
name: Server
element_kind: struct
change_kind: extend
tags:
  - solution/external-integration
  - element/internal-api-http-server-go
---

# Implementation changes
```go
type checkResponse struct {
	URL        string `json:"url"`
	Normalized string `json:"normalized"`
	// ... plus whatever field(s) this solution's port adds to {Result} — e.g.:
	Flagged bool   `json:"flagged"`
	Reason  string `json:"reason,omitempty"`
}

// in handleCheck, after calling s.service.{Method}:
code := http.StatusInternalServerError
switch {
case errors.Is(err, services.ErrInvalidURL):
	code = http.StatusBadRequest
case errors.Is(err, interfaces.ErrUnavailable):
	code = http.StatusBadGateway
}
```

# Rule changes

## MUST
- Every new field this solution's port adds to `{Result}` must appear in `checkResponse` — never left silently unreachable through the HTTP transport.
  - Risk: a caller integrating over HTTP has no way to observe data the domain service already computed, defeating the point of adding the port.
  - Fix: extend `checkResponse` with the new field(s) in the same change that extends the domain service.
- The port's own unavailable sentinel (`interfaces.ErrUnavailable`) must map to `http.StatusBadGateway` (a dependency of this service is unreachable), distinct from `http.StatusBadRequest` (the caller's own input was invalid).
  - Risk: mapping both to the same status code (or both to `500`) loses the distinction between "your request was wrong" and "we couldn't reach something we depend on," which a caller needs to decide whether retrying makes sense.
  - Fix: a `switch`/`case` per distinguishable sentinel error, as shown.

# Check list
- [ ] Every field `{Result}` gained is present in `checkResponse`.
- [ ] The port's unavailable sentinel maps to `502`, not `500` or `400`.
