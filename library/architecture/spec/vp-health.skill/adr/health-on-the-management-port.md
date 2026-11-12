---
name: health on the management port
description: Where a long-running service answers its health checks, whatever its entry points
problem: Services of the catalog may have an HTTP API, a gRPC API, a listener, or several of them; where do the orchestrator's probes go so that every service is probed the same way?
decision: A health variation point serves /health/live, /health/ready, and /health over HTTP on a port of its own (MANAGEMENT_LISTEN_PORT, 8081) in every long-running service; no API variation point serves health, and the gRPC health service is not used.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The HTTP variation point served the health endpoints on its API port. A gRPC-only or listener-only service had none, and the standard gRPC health service cannot be called by a Docker `healthcheck`. The probes of a deployment depended on which entry points the service had.

# Selected variant
[[#HTTP health on its own port, in every service]]
- Owner decision, 2026-10-18: one kind of health check for every service; health and the later metrics never published through the ingress.

# Searched variants

## HTTP health on its own port, in every service

**Selected.**

### Description
The health variation point is an entry point of its own: a small HTTP server on 8081 answering the three endpoints, added to the `entries` list beside any API. HTTP serves only its API on 8080, gRPC on 9090.

### Benefits
- Every service is probed alike: `:8081/health/*`, `curl` in a Docker `healthcheck`, `httpGet` in Kubernetes.
- The ingress routes only the API port, so `/health` and the later `/metrics` never reach a client.
- The HTTP and gRPC variation points carry no health code.

### Costs
- An HTTP service listens on two ports; the deployment exposes both and probes 8081.
- The deployment templates take the probe port as a placeholder.

## Health on the HTTP port when there is one, on its own port otherwise

### Description
The health variation point mounts its endpoints on the HTTP API's server when it is attached, and starts its own server only without one.

### Benefits
- An HTTP service keeps one port.

### Costs
- Two behaviours and two kinds of probes, chosen by which other variation points are attached — the combination the catalog exists to avoid.
- Health is published through the ingress with the API.

## gRPC health service beside HTTP health

### Description
The gRPC variation point also serves `grpc.health.v1`, used by Kubernetes `grpc` probes and gRPC-aware load balancers.

### Benefits
- gRPC clients and meshes can check health in their own protocol.

### Costs
- Two kinds of health check across the services of one system; a Docker `healthcheck` cannot call the gRPC one.
- Can be added later if a mesh needs it; nothing here prevents that.
