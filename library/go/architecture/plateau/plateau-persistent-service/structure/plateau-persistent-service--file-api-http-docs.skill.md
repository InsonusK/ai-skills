---
name: plateau-persistent-service--file-api-http-docs
description: internal/api/http/docs.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/api/http/docs.go, or changing how the OpenAPI spec and Swagger UI are served
domain: skill
type: template
plateau: plateau-persistent-service
version: 20261012000000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
---

# Goal
Serve the embedded contract and a Swagger UI for it from the binary, at URLs that work behind a path prefix.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/docs.go.create.md|docs.go]]

# Core Principles
- Apply ONE plateau template per file.
- The committed spec names no server; the prefix is set in the copy that is served.

# Implementation
```go
// Skill: file-api-http-docs
// Plateau: plateau-persistent-service
// Version: 20261012000000

package http

import (
	"encoding/json"
	"net/http"

	"github.com/swaggest/swgui"
	"github.com/swaggest/swgui/v5emb"

	"{module-path}/docs/integration/api/linkcheck"
)

// registerDocs adds the spec and Swagger UI routes. They are mounted at "/"
// like every route; each URL they hand to a browser carries basePath.
func registerDocs(mux *http.ServeMux, basePath string) {
	spec := publishedSpec(linkcheck.Spec, basePath)
	mux.HandleFunc("GET /openapi.json", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		_, _ = w.Write(spec)
	})

	ui := v5emb.NewHandlerWithConfig(swgui.Config{
		Title:            "Link check",
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

// publishedSpec is the embedded contract with its base path set to the path the
// service is published under, so Swagger UI's "Try it out" calls this service
// and not the ingress root. The generated file names none.
func publishedSpec(embedded []byte, basePath string) []byte {
	if basePath == "" {
		return embedded
	}
	var doc map[string]any
	if err := json.Unmarshal(embedded, &doc); err != nil {
		panic("the embedded OpenAPI contract is not JSON: " + err.Error())
	}
	// The generator writes OpenAPI v2, whose server prefix is basePath.
	doc["basePath"] = basePath
	published, err := json.Marshal(doc)
	if err != nil {
		panic(err)
	}
	return published
}
```
Verified against this plateau's own `examples/internal/api/http/docs.go` — the binary started with `HTTP_API_DOCS_ENABLED=true HTTP_BASE_PATH=/prefix` answers `/openapi.json` with `"basePath":"/prefix"`, `/swagger/` naming `/prefix/openapi.json`, and `/swagger` with a redirect to `/prefix/swagger/`.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/docs.go.create.md|docs.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `registerDocs` is called only from `Handler()`'s `if s.options.DocsEnabled` branch; this file reads no environment variable.
- `GET /swagger` is registered by hand beside `GET /swagger/` — the redirect `http.ServeMux` would add loses the prefix.
- `basePath` goes only into emitted values (the spec's `basePath`, `SwaggerJSON`, `BasePath`, the redirect) — never into a route pattern or `InternalBasePath`.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/docs.go.create.md#MUST|docs.go]]

# Check list
- [ ] No route pattern in this file contains `basePath`.
- [ ] `publishedSpec` returns the embedded bytes unchanged when `basePath` is empty.

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/docs.go.create.md|docs.go]]

# Unittest TestCases
- [ ] WHEN docs are on with no base path THEN `/openapi.json` and `/swagger/` answer 200
- [ ] WHEN docs are on under `/prefix` THEN the spec, the page, and the redirect each name `/prefix`
- [ ] WHEN docs are off THEN `/openapi.json` and `/swagger/` answer 404

__Applied solutions:__
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[library/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/api/http/docs.go.create.md|docs.go]]
