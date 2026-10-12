---
description: Add an HTTP endpoint exposing the persistence port's read side
project_name: internal/api/http
name: Server
element_kind: struct
change_kind: extend
tags:
  - solution/persistent-db
  - element/internal-api-http-server-go
---

# Implementation changes
The route joins the contract's list, and the contract gains its operation. With [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] applied the operation is the `rpc` of this solution's own [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md|grpc server.go.extend.md]] with its `google.api.http` annotation, and the handler uses its generated messages:
```go
func (s *Server) contractRoutes() []contractRoute {
	return []contractRoute{
		// ... existing routes ...
		{"GET /v1/{records}/recent", s.handle{ReadMethod}},
	}
}

func (s *Server) handle{ReadMethod}(w http.ResponseWriter, r *http.Request) {
	// A GET has no body: the request message is filled from the query.
	req := &apiv1.{ReadMethod}Request{}
	if v := r.URL.Query().Get("limit"); v != "" {
		n, err := strconv.ParseInt(v, 10, 32)
		if err != nil {
			writeError(w, status.New(codes.InvalidArgument, "limit: "+err.Error()))
			return
		}
		req.Limit = int32(n)
	}
	limit := int(req.GetLimit())
	if limit <= 0 {
		limit = 20
	}

	entries, err := s.service.{ReadMethod}(r.Context(), limit)
	if err != nil {
		st := toStatus(err)
		slog.Error("http: {readmethod} failed", "code", st.Code(), "error", err)
		writeError(w, st)
		return
	}

	out := make([]*apiv1.{Record}, len(entries))
	for i, e := range entries {
		out[i] = &apiv1.{Record}{Input: e.Input, CheckedAtUnix: e.CheckedAt.Unix()}
	}
	writeMessage(w, http.StatusOK, &apiv1.{ReadMethod}Response{Entries: out})
}
```
Without `solution-grpc-api` the handler answers its own structs with the same keys — `{"entries": [{"input": "…", "checkedAtUnix": "…"}]}`, the 64-bit number as a string, as proto-JSON writes it — and the path item is added to `openapi.json` by hand.

This catalog's own runnable example concretizes `{ReadMethod}` as `RecentChecks`, the route as `GET /v1/links/recent`, and `{Record}` as `HistoryEntry{normalized, flagged, reason, checked_at_unix}` — see `plateau-persistent-service`'s `examples/internal/api/http/server.go`.

# Rule changes

## MUST
- Add a route exposing `{Service}`'s `{ReadMethod}` — never leave the domain service's read method reachable only from within the process.
  - Risk: this solution's own Capability ("data ... survives a restart") is unverifiable by anything outside the process if no adapter exposes a way to read it back.
  - Fix: add the route in the same change that adds `{ReadMethod}` to the domain service (see this solution's own [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go.extend.md]]).
- Add the route as an entry of `contractRoutes()` and as an operation of the contract in one change, and bump the contract's minor version.
  - Violation: `mux.HandleFunc("GET /v1/…/recent", …)` in `Handler()`; the route served with no path in the spec.
  - Risk: the operations scenario cannot see a route outside the list, and Swagger UI does not show an endpoint the spec does not have.
  - Fix: the list entry, the annotation or path item, the version, and the scenario's table row together.
- Accept an optional, bounded `limit` query parameter with a sane default (this catalog's example uses 20) — never return an unbounded result set by default — and answer a `limit` that is not a number with `INVALID_ARGUMENT`.
  - Risk: a store that has accumulated a large number of records returns all of them on every call with no limit, degrading response time and payload size as the table grows; a silently ignored bad value hides the caller's mistake.
  - Fix: default to a small limit, let the caller widen it explicitly, refuse what cannot be parsed.
- Answer the list as an object with one repeated field — never a bare JSON array.
  - Risk: a top-level array cannot gain a field (a cursor, a total) without breaking every client, and no proto message maps to it.
  - Fix: `{"entries": […]}`.

# Check list
- [ ] A `GET` route of `contractRoutes()` exposes `{Service}`'s `{ReadMethod}`, and the contract describes it.
- [ ] The route accepts a bounded, optionally caller-supplied `limit`; `limit=abc` answers 400 with `code` 3.
- [ ] The response body is an object.
