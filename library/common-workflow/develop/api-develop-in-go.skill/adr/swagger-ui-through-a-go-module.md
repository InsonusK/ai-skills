---
name: swagger-ui-through-a-go-module
description: Decide where a Go service takes the Swagger UI page and its assets from.
problem: `api-develop` requires a Swagger UI for every HTTP endpoint, and a Go service has no web framework that brings one — where do the page, its scripts, and its styles come from?
decision: The Go module `github.com/swaggest/swgui`, package `v5emb`, pinned in `go.mod`; its assets are embedded in the binary.
tags:
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Swagger UI is a page with about 1.5 MB of scripts and styles. `api-develop` asks that the page works under a path prefix (`HTTP_BASE_PATH`) and is switched per deployment. `net/http` brings none of it.

# Selected variant
**Selected variant:** [[#Go module with embedded assets]]
- The page works with no network beyond the service, at a version `go.mod` pins.

# Searched variants

## Go module with embedded assets

**Selected.**

### Description
`github.com/swaggest/swgui/v5emb` embeds Swagger UI 5 and returns an `http.Handler`. Its `Config.BasePath` is the path the browser asks for and `Config.InternalBasePath` the path that reaches the service — the two sides of an ingress that strips a prefix.

### Benefits
- Works in a closed network; the page loads nothing from another host.
- The version is one line of `go.mod`, updated like any dependency.
- External and internal base path are separate settings, so the routes stay mounted at `/`.
- Verified in a running service under `HTTP_BASE_PATH=/lc`: the page, its assets, and the spec URL all carry the prefix.

### Costs
- The binary grows by the assets: 9.1 MB to 14.4 MB, measured on a service with one endpoint.
- One more dependency, with two of its own (`statigz`, `brotli`).

## CDN with a pinned version

### Description
A small HTML page written in the service references `swagger-ui-dist@{version}` on a CDN, with integrity hashes.

### Benefits
- Nothing added to the binary or to `go.mod`.

### Costs
- The page does not open when the browser cannot reach the CDN.
- A page of an internal service runs a script from another host.

## Assets committed to the service

### Description
The files of `swagger-ui-dist` are committed and embedded with `//go:embed`.

### Benefits
- No dependency and no network.

### Costs
- Megabytes of another project's code in every service's repository.
- An update is a manual copy that nothing reminds of.
