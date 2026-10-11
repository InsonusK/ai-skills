---
name: stack-healthcheck-probes-liveness
description: Decide which health endpoint the single Docker healthcheck calls on Docker Stack and on Docker Compose.
problem: Docker has one healthcheck per container where Kubernetes has two probes — does it call /health/live or /health/ready?
decision: The Docker Stack healthcheck calls /health/live; the Docker Compose healthcheck calls /health/ready.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A container has one `healthcheck`, and the two Docker paths act on its result differently. Swarm kills and replaces a task whose container is unhealthy, and counts the replacement against `restart_policy.max_attempts`. Docker Compose never restarts an unhealthy container; it only reports the state and lets another service wait for it with `depends_on: condition: service_healthy`. The endpoint has to be chosen for each path, see [[./separate-liveness-and-readiness-endpoints.md|separate-liveness-and-readiness-endpoints]] for the endpoints.

# Selected variant
**Selected variant:** [[#Liveness on Stack, readiness on Compose]]
- Each path gets the endpoint that matches what the platform does with the result.

# Searched variants

## Liveness on Stack, readiness on Compose

**Selected.**

### Description
`docker-stack.example.yml` calls `/health/live`; `docker-compose.example.yml` calls `/health/ready`.

### Benefits
- A dependency outage does not make Swarm kill every replica, nor use up `restart_policy.max_attempts` and leave the service down after the dependency is back.
- On Compose, `healthy` means the service can serve, which is what a dependent service and a person reading `docker compose ps` expect.

### Costs
- On Stack, a rolling update counts a new replica as healthy before its dependencies are ready; the guide's verification step calls `/health/ready` to cover that.
- The two Docker manifests call different endpoints, which reads as an oversight without the comment in the Stack manifest.

## Readiness on both

### Description
Both manifests call `/health/ready`.

### Benefits
- One endpoint for both Docker paths.
- A Swarm rolling update with `failure_action: rollback` waits for a replica that can really serve.

### Costs
- A dependency outage makes Swarm restart every replica, and after `max_attempts` the service stays down.

## Liveness on both

### Description
Both manifests call `/health/live`.

### Benefits
- One endpoint for both Docker paths, and no restart on a dependency outage.

### Costs
- On Compose, `service_healthy` lets a dependent service start against a service that cannot serve yet.
