---
name: vp-health
description: The health variation point — a management entry point attached to the base plateau that answers the orchestrator's probes with /health/live, /health/ready, and /health on a port of its own, the same in every long-running service whatever its other entry points, runs the readiness checks the service variation points add, and is never published through the ingress
whenToUse: when an application built on the base plateau runs as a long-running service (it has an HTTP, gRPC, or listener entry point), when a service variation point adds a readiness check, when you write the probes of such a service's deployment, or when you review its health endpoints
updated: 20261021
tags:
  - stack
  - concern/architecture
  - concern/ci
  - app-type/service
adr:
  - adr/health-on-the-management-port.md
---

# Goal
- **One health check for every service** - Every long-running service answers the same three endpoints on the same port, whether its API is HTTP, gRPC, a listener, or several of them, so a deployment's probes never depend on the service's kind.
- **Probe-ready** - An orchestrator probes liveness and readiness separately: liveness restarts only a hung process, readiness holds traffic back while a dependency is down.
- **Not published** - The health endpoints, and the metrics that will join them, never reach a client through the ingress.
- **Attached, not edited** - The entry point joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) as one more entry of the `entries` list and changes no other base file but the composition root and the readme.

# Core Principle
- **A port for the operator, a port for the client** - The management port serves the orchestrator and the operator; the API port serves clients; an ingress routes only the API port.
- **Readiness is the sum of the dependencies** - The entry point has no check of its own; each service variation point brings the check of the dependency it adds.

# Scope
- Covers the health endpoints of a long-running service. A one-shot application (the startup command alone, a CLI) has no probes; attached to one, the entry point stops when the command is done.
- `/metrics` is planned on the same port with the metrics variation point.
- The implementation for each stack is described in its stack-specialized extension: `vp-health-in-go`, `vp-health-in-dotnet`, `vp-health-in-python`.

# Rule

## MUST

### Serve the three endpoints
Serve `/health/live`, `/health/ready`, and `/health` as [devops-service-deploy](library/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) defines them: liveness checks nothing, readiness answers 503 and names every readiness check that fails, `/health` carries the status and the version of the running binary.
- Risk: without separate probes an orchestrator restarts a healthy process during a database outage or sends traffic to a replica that is not ready.
- Fix: liveness answers 200 while the process answers; readiness runs every registered check.

### Listen on the management port
Serve the endpoints on a port of their own, `MANAGEMENT_LISTEN_PORT` (default `8081`), never on the API port, and point every probe and `healthcheck` of the deployment at it.
- Violation: `/health/*` served beside the API routes on 8080; a gRPC-only service probed through the gRPC health service; a probe that names the API port.
- Risk: health on the API port is published through the ingress to every client, and a service without an HTTP API has no health check that a Docker `healthcheck` can call.
- Fix: the management entry point listens on 8081 in every service; the ingress routes only the API port; the probes name `{health-port}` = 8081 in the deployment templates. Decision recorded in [health-on-the-management-port](./adr/health-on-the-management-port.md).

### Take the readiness checks from the composition root
Accept readiness checks — a name and a check function — from the composition root, so that a service variation point adds its dependency's check without editing the health entry point.
- Risk: a readiness endpoint that knows its dependencies must be edited by every service variation point.
- Fix: the entry point takes a list of checks; the composition root passes one per dependency.

### Stop gracefully
On termination (`SIGTERM`), stop accepting requests, let requests in flight finish within a bounded time, and return without an error.
- Risk: a probe in flight during a rolling update is cut off and the replica is marked failed.
- Fix: serve until the composition root's context is cancelled, then shut down with a timeout.

### Read its own settings
Read `MANAGEMENT_LISTEN_PORT` in the management entry point's own package, rejecting an unusable value with an error naming the variable.
- Risk: a setting added to the base settings outlives the entry point and makes the variation point edit a base file.
- Fix: a settings loader in the entry point, called by its one start call.

### Attach as one more entry
Attach the entry point by adding its package and one entry to the composition root's `entries` list, keeping every other entry — the startup command, HTTP, gRPC — as it is, and changing no other base file than the composition root and the readme.
- Risk: a health variation point that replaces or wraps another entry point has to be described together with each of them.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Test the endpoints with fake checks
Test liveness with a failing check, readiness with passing and failing checks, `/health` with a set version, the settings, and the graceful stop.
- Risk: a readiness that ignores a failing check, or a liveness that runs one, reaches production unnoticed.
- Fix: a check is a function the scenario sets to pass or fail.

# Check list
- [ ] `/health/live`, `/health/ready`, and `/health` answer on `MANAGEMENT_LISTEN_PORT` (8081), not on the API port.
- [ ] Liveness checks nothing; readiness runs every check the composition root passed and names the failed ones with 503.
- [ ] Every probe and `healthcheck` of the deployment names the management port; the ingress does not route it.
- [ ] The server stops gracefully on termination.
- [ ] The entry point is one more entry of the `entries` list; no other entry and no other base file but the composition root and the readme changed.
