---
name: vp-health-in-go
description: The health variation point in Go — the internal/api/management package copied from templates (MANAGEMENT_LISTEN_PORT, /health/live, /health/ready, /health, readiness checks, graceful stop) and one management entry added to the entry.RunAll list in main.go
whenToUse: when a Go application built on the base plateau runs as a long-running service, when a Go service variation point adds a readiness check, or when you review a Go service's health endpoints
updated: 20261018
tags:
  - stack/go
  - concern/architecture
  - concern/ci
  - app-type/service
---

# Goal
- **The health variation point in Go** - A Go application that keeps every property of [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example.

# Core Principle
- **Copied whole, one line in main.go** - `internal/api/management/` is copied from templates and never edited; the application adds one `entry.Entry` and, per dependency, one `management.Check`.

```
internal/api/management/      copied from templates, never edited
├── config.go                 MANAGEMENT_LISTEN_PORT (8081)
├── server.go                 Run, Check, /health/live /health/ready /health, graceful stop
└── features/  test/          the scenarios, copied
```

# Rule

## MUST

### Copy the entry point from templates
Fill and copy [config.go](./templates/internal/api/management/config.go) and [server.go](./templates/internal/api/management/server.go) to `internal/api/management/`, with the example's `features/` and `test/`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped server drifts in its readiness answer or shutdown from every other Go service of the catalog.
- Fix: copy the files; after filling, searching them for `{module-path}` returns nothing; `make test` passes.

### Add the management entry in main.go
Add a `management` `entry.Entry` calling `management.Run` to the `// [entries]` list of `cmd/{app}/main.go`, beside the entries already there.

```go
// [entries] - every entry point, one Entry each, run together by entry.RunAll.
err = entry.RunAll(ctx, logger,
	entry.Entry{Name: "http", Run: func(ctx context.Context) error { // already there
		return httpapi.Run(ctx, routes, logger)
	}},
	entry.Entry{Name: "management", Run: func(ctx context.Context) error {
		return management.Run(ctx, logger)
	}},
)
return entry.ExitCode(err)
```

- Violation: the health endpoints registered on the HTTP API's server; `management.Run` called outside `entry.RunAll`; another entry removed or changed.
- Risk: health served on the API port is published through the ingress; a server started outside the list keeps the process half-alive when another entry point fails.
- Fix: import `"{module-path}/internal/api/management"`; one entry, nothing else in `main.go`.

### Add a readiness check as one argument
Pass a dependency's readiness check as a `management.Check{Name, Check}` argument of `management.Run`, built in `// [services]` by the service variation point that brings the dependency.

```go
return management.Run(ctx, logger,
	management.Check{Name: "database", Check: db.Ping},
)
```

- Violation: a check written inside `internal/api/management`; a check that calls a command.
- Risk: the copied package must be edited for every dependency, and the next copy of the templates loses the check.
- Fix: the check is a method of the dependency's infrastructure (`Ping`), passed in by the composition root.

### Probe the management port
Fill the deployment templates of [devops-service-deploy](library/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) with `{health-port}` = `8081`, and expose `8081` from the container without routing it through the ingress.
- Risk: probes left on 8080 fail on a gRPC-only service and pass through the API port on an HTTP one.
- Fix: every probe, `healthcheck`, and verification `curl` names 8081.

# Check list
- [ ] `internal/api/management/{config,server}.go` are filled copies of the templates; no placeholder is left.
- [ ] The `// [entries]` list of `main.go` has a `management` entry calling `management.Run`; every other entry is unchanged.
- [ ] Every readiness check is a `management.Check` argument of `management.Run`.
- [ ] The deployment's probes name port 8081.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the health endpoints attached: `make run`, `make test`.
