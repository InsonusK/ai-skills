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
```go
func (s *Server) Handler() http.Handler {
	mux := http.NewServeMux()
	// ... existing routes ...
	mux.HandleFunc("GET /v1/{records}/recent", s.handle{ReadMethod})
	return mux
}

type {record}Entry struct {
	Input     string    `json:"input"`
	CheckedAt time.Time `json:"checked_at"`
}

func (s *Server) handle{ReadMethod}(w http.ResponseWriter, r *http.Request) {
	limit := 20
	if v := r.URL.Query().Get("limit"); v != "" {
		if n, err := strconv.Atoi(v); err == nil && n > 0 {
			limit = n
		}
	}

	entries, err := s.service.{ReadMethod}(r.Context(), limit)
	if err != nil {
		writeError(w, http.StatusInternalServerError, err)
		return
	}

	out := make([]{record}Entry, len(entries))
	for i, e := range entries {
		out[i] = {record}Entry{Input: e.Input, CheckedAt: e.CheckedAt}
	}

	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(out)
}
```
This catalog's own runnable example concretizes `{ReadMethod}` as `RecentChecks`, the route as `GET /v1/links/recent`, and `{record}Entry` as `historyEntry{Normalized, Flagged, Reason, CheckedAt}` — see `plateau-persistent-service`'s `example/internal/api/http/server.go`.

# Rule changes

## MUST
- Add a route exposing `{Service}`'s `{ReadMethod}` — never leave the domain service's read method reachable only from within the process.
  - Risk: this solution's own Capability ("data ... survives a restart") is unverifiable by anything outside the process if no adapter exposes a way to read it back.
  - Fix: add the route in the same change that adds `{ReadMethod}` to the domain service (see this solution's own [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go.extend.md]]).
- Accept an optional, bounded `limit` query parameter with a sane default (this catalog's example uses 20) — never return an unbounded result set by default.
  - Risk: a store that has accumulated a large number of records returns all of them on every call with no limit, degrading response time and payload size as the table grows.
  - Fix: default to a small limit, let the caller widen it explicitly.

# Check list
- [ ] A `GET` route exposes `{Service}`'s `{ReadMethod}`.
- [ ] The route accepts a bounded, optionally caller-supplied `limit`.
