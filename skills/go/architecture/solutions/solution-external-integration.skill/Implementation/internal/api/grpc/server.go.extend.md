---
description: Surface the port's result in the gRPC response, and map the port's unavailable sentinel to a status code (conditional — only if solution-grpc-api is also applied)
project_name: internal/api/grpc
name: Server
element_kind: struct
change_kind: extend
tags:
  - solution/external-integration
  - element/internal-api-grpc-server-go
---

# Implementation changes
Applies only when [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] is also composed on the same plateau — `internal/api/grpc` does not exist otherwise. When it is:
```go
// {Method}Response in the .proto gains the same new field(s) {Result} gained (see
// solution-grpc-api's own Repository.extend.md for the message shape) — e.g.:
//   bool flagged = 3;
//   string reason = 4;

// in Server.{Method}, after calling s.service.{Method}:
return &apiv1.{Method}Response{
	// ... existing fields ...
	Flagged: result.Flagged,
	Reason:  result.Reason,
}, nil

func toStatus(err error) error {
	switch {
	case errors.Is(err, services.ErrInvalidURL):
		return status.Error(codes.InvalidArgument, err.Error())
	case errors.Is(err, interfaces.ErrUnavailable):
		return status.Error(codes.Unavailable, err.Error())
	default:
		return status.Error(codes.Internal, err.Error())
	}
}
```

# Rule changes

## MUST
- Every new field this solution's port adds to `{Result}` must appear in the generated response message — never left silently unreachable through the gRPC transport, the same requirement [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/api/http/server.go.extend.md|this solution's HTTP-adapter extension]] states.
  - Risk: a caller integrating over gRPC has no way to observe data the domain service already computed.
  - Fix: extend the `.proto` message and the mapping in `Server.{Method}` together.
- The port's own unavailable sentinel must map to `codes.Unavailable` — gRPC's own status code for "a dependency is unreachable," distinct from `codes.InvalidArgument`.
  - Risk: collapsing both into `codes.Internal` loses the distinction a gRPC client's own retry logic typically keys on (`codes.Unavailable` is conventionally retryable, `codes.InvalidArgument` is not).
  - Fix: a `switch`/`case` per distinguishable sentinel error, mirroring the HTTP adapter's status mapping in kind, not in the exact codes used (each transport has its own vocabulary).

# Check list
- [ ] If `solution-grpc-api` is applied, every field `{Result}` gained is present in the generated response message.
- [ ] The port's unavailable sentinel maps to `codes.Unavailable`, not `codes.Internal` or `codes.InvalidArgument`.
