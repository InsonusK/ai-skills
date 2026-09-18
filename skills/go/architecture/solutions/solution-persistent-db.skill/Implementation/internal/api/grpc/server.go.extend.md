---
description: Add a gRPC RPC exposing the persistence port's read side (conditional — only if solution-grpc-api is also applied)
project_name: internal/api/grpc
name: Server
element_kind: struct
change_kind: extend
tags:
  - solution/persistent-db
  - element/internal-api-grpc-server-go
---

# Implementation changes
Applies only when [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] is also composed on the same plateau — `internal/api/grpc` does not exist otherwise. When it is, `proto/{service}/{service}.proto` (created by `solution-grpc-api`'s own `Repository.extend.md`) gains a new RPC and its messages — an ordinary proto extension, not a new proto file (an external contract like `solution-external-integration`'s would get its own file; this is the module's *own* contract growing):
```protobuf
service {Service} {
  // ... existing rpc ...
  rpc {ReadMethod}({ReadMethod}Request) returns ({ReadMethod}Response);
}

message {ReadMethod}Request {
  int32 limit = 1;
}

message {Record} {
  string input = 1;
  int64 checked_at_unix = 2;
}

message {ReadMethod}Response {
  repeated {Record} entries = 1;
}
```

```go
func (s *Server) {ReadMethod}(ctx context.Context, req *apiv1.{ReadMethod}Request) (*apiv1.{ReadMethod}Response, error) {
	limit := int(req.GetLimit())
	if limit <= 0 {
		limit = 20
	}

	entries, err := s.service.{ReadMethod}(ctx, limit)
	if err != nil {
		return nil, toStatus(err)
	}

	out := make([]*apiv1.{Record}, len(entries))
	for i, e := range entries {
		out[i] = &apiv1.{Record}{
			Input:         e.Input,
			CheckedAtUnix: e.CheckedAt.Unix(),
		}
	}
	return &apiv1.{ReadMethod}Response{Entries: out}, nil
}
```
This catalog's own runnable example concretizes `{ReadMethod}` as `RecentChecks` and `{Record}` as the proto message `HistoryEntry{normalized, flagged, reason, checked_at_unix}` — see `plateau-persistent-service`'s `example/proto/linkcheck/linkcheck.proto` and `example/internal/api/grpc/server.go`.

# Rule changes

## MUST
- Add an RPC exposing `{Service}`'s `{ReadMethod}`, mirroring [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|this solution's HTTP-adapter extension]] — the same capability must be reachable from every applied transport, not just whichever one was extended first.
  - Risk: a port readable over HTTP but not gRPC (or vice versa) makes the durable data's availability depend on which transport a given caller happens to use, which nothing in this solution's contract promises.
  - Fix: extend the `.proto` and `Server.{ReadMethod}` together, in the same change.
- Bound the RPC's `limit` the same way the HTTP route does (see that file's own MUST) — a `0` or negative `limit` falls back to a sane default, never an unbounded query.

# Check list
- [ ] If `solution-grpc-api` is applied, an RPC exposes `{Service}`'s `{ReadMethod}`.
- [ ] The RPC's `limit` falls back to a bounded default when unset or non-positive.
