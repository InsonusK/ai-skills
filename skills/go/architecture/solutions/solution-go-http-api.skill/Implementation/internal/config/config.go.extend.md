---
description: Add HTTPListenPort, HTTPAPIDocsEnabled and HTTPBasePath to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/go-http-api
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	HTTPListenPort string
	// HTTPAPIDocsEnabled registers the spec and Swagger UI routes; off unless set.
	HTTPAPIDocsEnabled bool
	// HTTPBasePath is the path prefix the service is published under: empty, or
	// with a leading "/" and no trailing one.
	HTTPBasePath string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		HTTPListenPort:     l.get("HTTP_LISTEN_PORT", "8080"),
		HTTPAPIDocsEnabled: l.getBool("HTTP_API_DOCS_ENABLED", false),
		HTTPBasePath:       normalizeBasePath(l.get("HTTP_BASE_PATH", "")),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}

// normalizeBasePath gives a path prefix its one form: "" for none, otherwise a
// leading "/" and no trailing one ("orders/" and "/orders" both become "/orders").
func normalizeBasePath(path string) string {
	path = strings.Trim(strings.TrimSpace(path), "/")
	if path == "" {
		return ""
	}
	return "/" + path
}
```

# Rule changes

## MUST
- Read the docs switch from `HTTP_API_DOCS_ENABLED` and the prefix from `HTTP_BASE_PATH` — the names [[skills/common-workflow/develop/api-develop.skill/api-develop.skill.md|api-develop]] fixes — and nothing else.
  - Violation: `SWAGGER_ON`; docs enabled when `LOG_LEVEL=debug`.
  - Risk: an operator cannot switch docs across a fleet without reading each service's code.
  - Fix: the two `Config` fields above.

# Check list
- [ ] `HTTP_LISTEN_PORT` defaults to `8080` when unset.
- [ ] `HTTP_API_DOCS_ENABLED` defaults to `false`; a value `strconv.ParseBool` rejects fails `Load()`.
- [ ] `HTTP_BASE_PATH` values `orders`, `/orders`, and `/orders/` all load as `/orders`; unset loads as empty.
