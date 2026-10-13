---
description: The one place the version is recorded — the variable the running program reports, overridable via -ldflags for a snapshot build
project_name: internal/version
name: version
element_kind: functions
change_kind: create
tags:
  - solution/go-repository-structure
  - element/internal-version-version-go
---

# Goals
- Record the project's version in one place, the variable the running program reports.
- Make the running binary's version observable to its consumer — a service in its health response, a CLI/desktop app via `--version`.

# Core Principles
- `var Version` in this file is the only source of the version value: `make version` reads it and a plain `go build` carries it (see the ADR `version-source-file` of `devops-project-version-in-go`). There is no `VERSION` file.
- `-ldflags -X` only overrides: a snapshot build replaces the recorded version, a release build passes no flag.

# Implementation changes
```go
// Package version holds the version of the program.
package version

// Version is the project's version, recorded here and nowhere else: `make version` reads
// this line. A snapshot build overrides it with
//   -ldflags "-X {module-path}/internal/version.Version={version}"
var Version = "0.1.0"
```

Dockerfile builder stage (when the service ships as an image):
```dockerfile
FROM golang:1.26 AS build
ARG VERSION=
WORKDIR /src
COPY . .
RUN go build ${VERSION:+-ldflags "-X {module-path}/internal/version.Version=${VERSION}"} -o /out/{service} ./cmd/{service}

FROM gcr.io/distroless/static
ARG VERSION=
LABEL org.opencontainers.image.version="${VERSION}"
COPY --from=build /out/{service} /{service}
ENTRYPOINT ["/{service}"]
```

# Rule

## MUST
- Record the version only as `var Version = "MAJOR.MINOR.PATCH"` on one line of this file; keep no `VERSION` file and no other copy of the number.
  - Risk: a second source drifts from this one, and a `const` or a moved declaration is not found by `tools/version/read-version.sh`.
  - Fix: one `var` line here; raise it in the pull request that changes what is shipped.
- Expose the version to the consumer: a service returns it as the `version` field of its health response, a CLI/desktop app prints it for `--version`.
  - Risk: with no reader, `Version` is dead code that invites deletion, and nobody can tell which build is running.
  - Fix: read `version.Version` in the health handler or the `--version` flag.
- In a Dockerfile, declare `ARG VERSION=` in the builder stage and add `-ldflags "-X {module-path}/internal/version.Version=${VERSION}"` to `go build` only when it is not empty; declare `ARG VERSION` again in the final stage only to set `org.opencontainers.image.version`.
  - Violation: `ARG VERSION=dev` always passed to `go build`.
  - Risk: an image built without the argument reports `dev` instead of the recorded version.
  - Fix: use the builder stage above.

# Check list
- [ ] `Version` holds the project's version; `make -s version` prints it.
- [ ] No `VERSION` file and no other Go file holds the version number.
- [ ] `go build -ldflags "-X {module-path}/internal/version.Version=9.9.9" ...` overrides `Version`.
- [ ] A service's health response carries `version`; a CLI/desktop app prints it for `--version`.
- [ ] The Dockerfile's builder stage passes `VERSION` to `-ldflags` only when it is set.
