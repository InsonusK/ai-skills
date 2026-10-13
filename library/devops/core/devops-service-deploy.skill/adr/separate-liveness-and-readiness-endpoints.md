---
name: separate-liveness-and-readiness-endpoints
description: Decide which HTTP health endpoints a deployable service exposes and what each one checks.
problem: Which health endpoints does a service expose so that an orchestrator can both restart a hung process and hold traffic back from a replica whose dependencies are not ready?
decision: Expose /health/live without dependency checks and /health/ready with them, beside the service's /health.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
An orchestrator asks a service two different questions: "is the process stuck, so that it must be restarted?" and "can this replica take traffic now?". The skill answered both with one `/health-check` endpoint that reported the process together with its dependencies, and pointed the Kubernetes `livenessProbe` and `readinessProbe` at it. With that endpoint a database outage fails the liveness probe of every replica, and Kubernetes restarts them all, although a restart cannot repair the database. The path also differed from `GET /health`, which the service skills of this repository implement and the Angular plateaus poll, so a service built by one skill failed the probes written by the other.

# Selected variant
**Selected variant:** [[#/health/live and /health/ready beside /health]]
- A dependency outage takes a replica out of rotation instead of restarting it.
- `/health` stays what the service skills and the frontends already use.

# Searched variants

## /health/live and /health/ready beside /health

**Selected.**

### Description
`/health/live` succeeds whenever the process answers and checks no dependency; `/health/ready` succeeds when the process and every dependency it needs to serve a request are ready. `/health` remains the endpoint for clients and people, carrying the status and version, and no probe calls it.

### Benefits
- The liveness probe restarts only a process that is really stuck.
- The readiness probe holds traffic back during start-up and during a dependency outage.
- The names are the ones MicroProfile Health defines, and they nest under the `/health` the repository already uses.

### Costs
- Three routes to implement and keep outside the API contract instead of one.
- Every service skill that implements `/health` must add the two probe routes.

## One /health-check endpoint

### Description
One endpoint that reports the process together with its dependencies, called by every probe. This is what the skill required before.

### Benefits
- One route to implement.
- Every manifest names the same path.

### Costs
- A dependency outage fails liveness and restarts every replica.
- The path matches nothing else in the repository.

## One /health endpoint

### Description
The same single endpoint under the path the service skills already implement.

### Benefits
- One route, already implemented by the service skills.

### Costs
- Either it checks dependencies and restarts every replica on an outage, or it does not and a replica takes traffic before it is ready.

## /livez and /readyz

### Description
The two probe endpoints under the names the Kubernetes components use for themselves, beside `/healthz`.

### Benefits
- Familiar to Kubernetes operators and common in Go services.

### Costs
- `/healthz` would replace the `/health` the service skills and the Angular plateaus already use.
- The `z` suffix says nothing to a reader outside the Kubernetes ecosystem.
