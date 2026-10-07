---
description: Build-time version receiver, filled from the root VERSION file via -ldflags at build time
project_name: internal/version
name: version
element_kind: functions
change_kind: create
tags:
  - solution/go-repository-structure
  - element/internal-version-version-go
---

# Goals
- Make the running binary's version observable to its consumer — a service in its health response, a CLI/desktop app via `--version` — without hand-editing a file per release.

# Core Principles
- The root `VERSION` file is the only source of the version value (see ADR [[skills/go/devops/devops-github-action-check-version-in-go.skill/adr/version-source-file.md|version-source-file]]). `Version` here is only the in-binary receiver: `"dev"` in source, overwritten at build time via `-ldflags -X`. It is not the "version.go constant" that ADR rejects — that variant meant a constant holding the real number.
- Every build path — `make build`, the Docker image, a release binary — injects the same variable, `{module-path}/internal/version.Version`.

# Implementation changes
```go
// Package version holds the build-time version string.
package version

// Version is overridden at build time from the root VERSION file via:
//   -ldflags "-X {module-path}/internal/version.Version=$(VERSION)"
// Never write a version number here.
var Version = "dev"
```

Dockerfile builder stage (when the service ships as an image):
```dockerfile
FROM golang:1.26 AS build
ARG VERSION=dev
WORKDIR /src
COPY . .
RUN go build -ldflags "-X {module-path}/internal/version.Version=${VERSION}" -o /out/{service} ./cmd/{service}

FROM gcr.io/distroless/static
ARG VERSION=dev
LABEL org.opencontainers.image.version="${VERSION}"
COPY --from=build /out/{service} /{service}
ENTRYPOINT ["/{service}"]
```

# Rule

## MUST
- Take the version only from the root `VERSION` file, injected via `-ldflags "-X {module-path}/internal/version.Version=..."`; never write a version number in Go source.
  - Risk: a number in source is a second version source that drifts from `VERSION`, and readers delete one of the two.
  - Fix: keep `Version = "dev"` and inject the real value at build time.
- Expose the version to the consumer: a service returns it as the `version` field of its health response, a CLI/desktop app prints it for `--version`.
  - Risk: with no reader, `Version` is dead code that invites deletion, and nobody can tell which build is running.
  - Fix: read `version.Version` in the health handler or the `--version` flag.
- In a Dockerfile, declare `ARG VERSION=dev` in the builder stage and pass `-ldflags "-X {module-path}/internal/version.Version=${VERSION}"` to `go build`; declare `ARG VERSION` again in the final stage only to set `org.opencontainers.image.version`.
  - Violation: `ARG VERSION` declared but never passed to `go build`, or declared only in the final stage.
  - Risk: the image builds with `Version = "dev"` and reports `"dev"` in production.
  - Fix: use the builder stage above.

# Check list
- [ ] `go build -ldflags "-X {module-path}/internal/version.Version=1.2.3" ...` overrides `Version` at build time.
- [ ] No Go source file contains a version number; `Version` is `"dev"` in source.
- [ ] A service's health response carries `version`; a CLI/desktop app prints it for `--version`.
- [ ] The Dockerfile's builder stage declares `ARG VERSION` and passes it to `-ldflags`.
