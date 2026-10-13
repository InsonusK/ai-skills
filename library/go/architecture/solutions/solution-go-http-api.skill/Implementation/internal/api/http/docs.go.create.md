---
description: The spec and Swagger UI routes of the HTTP adapter
project_name: internal/api/http
name: docs
element_kind: functions
change_kind: create
tags:
  - solution/go-http-api
  - element/internal-api-http-docs-go
---

# Goals
- Serve the embedded contract and a Swagger UI for it from the binary, at URLs that work behind a path prefix.

# Core Principles
- The committed spec names no server: the prefix is one deployment's value, set in the copy that is served.

# Implementation changes
```go
package http

import (
	"encoding/json"
	"net/http"

	"github.com/swaggest/swgui"
	"github.com/swaggest/swgui/v5emb"

	"{module-path}/docs/integration/api/{contract-name}"
)

// registerDocs adds the spec and Swagger UI routes. They are mounted at "/"
// like every route; each URL they hand to a browser carries basePath.
func registerDocs(mux *http.ServeMux, basePath string) {
	spec := publishedSpec({contract-package}.Spec, basePath)
	mux.HandleFunc("GET /openapi.json", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write(spec)
	})

	ui := v5emb.NewHandlerWithConfig(swgui.Config{
		Title:            "{title}",
		SwaggerJSON:      basePath + "/openapi.json",
		BasePath:         basePath + "/swagger/",
		InternalBasePath: "/swagger/",
	})
	mux.Handle("GET /swagger/", ui)
	// Registered by hand: the redirect ServeMux would add for "/swagger" points
	// at "/swagger/" and loses the prefix.
	mux.HandleFunc("GET /swagger", func(w http.ResponseWriter, r *http.Request) {
		http.Redirect(w, r, basePath+"/swagger/", http.StatusMovedPermanently)
	})
}

// publishedSpec is the embedded contract with its server URL set to the path
// the service is published under, so Swagger UI's "Try it out" calls this
// service and not the ingress root. The committed file names no server.
func publishedSpec(embedded []byte, basePath string) []byte {
	if basePath == "" {
		return embedded
	}
	var doc map[string]any
	if err := json.Unmarshal(embedded, &doc); err != nil {
		panic("the embedded OpenAPI contract is not JSON: " + err.Error())
	}
	doc["servers"] = []map[string]string{{"url": basePath}}
	published, err := json.Marshal(doc)
	if err != nil {
		panic(err)
	}
	return published
}
```

# Rule changes

## MUST
- Register the docs routes only from `Handler()`'s `if s.options.DocsEnabled` branch.
  - Violation: `registerDocs` called unconditionally; a second switch read from the environment inside this file.
  - Risk: production hands a map of its API to anyone who reaches it, or an operator's one switch no longer decides.
  - Fix: the single call in `Handler()`; this file reads no environment variable.
- Register `GET /swagger` by hand beside `GET /swagger/`.
  - Risk: `http.ServeMux` answers `/swagger` on its own with a redirect to `/swagger/`, which behind an ingress prefix is the ingress root.
  - Fix: the explicit handler redirecting to `basePath + "/swagger/"`.
- Put `basePath` only into emitted values — `SwaggerJSON`, `BasePath`, the redirect, the served spec's `servers` — never into a route pattern or `InternalBasePath`.
  - Risk: the ingress strips the prefix before forwarding, so a route mounted under it answers 404 in the cluster and 200 locally.
  - Fix: patterns and `InternalBasePath` start at `/`.

# Check list
- [ ] No route pattern in this file contains `basePath`.
- [ ] `publishedSpec` returns the embedded bytes unchanged when `basePath` is empty.
