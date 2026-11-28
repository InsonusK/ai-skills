# Probe readiness on the published health port

Status: Ready

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

Deployment verification guides curl /health/ready through {host-port}, the
published API port. Architecture services serve health on a separate management
port. The owning skill already defines {health-port}, and runtime probes use it,
but the operator's post-deployment verification can still target the wrong port.

## Sources and scope

- [Deployment contract and port placeholders](../../devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md)
- [Deployment guide and manifest templates](../../devops/core/devops-service-deploy.skill/templates)
- [Architecture health contract](../spec/vp-health.skill)

Inspect the k8s, Helm, Docker Compose and Docker Stack deployment guides under
the template directory. Change related port publication/forwarding instructions
only where needed for a working verification command. Preserve each deployment
mode's live/readiness probe semantics.

## Dependencies and start condition

Can start independently now. Confirm the distinction between container
health port, published host port and a port-forward's local port in each guide.

## Expected result

1. Each guide tells the operator exactly how /health/ready is reached when
   health runs on a separate management port.
2. Guides remain valid when health shares the API port. A host curl does not
   substitute an unpublished container-only port.
3. Supply a concrete separate-port example and a command whose port matches the
   publication or port-forward configured by that guide. Do not expose a
   management port publicly by default merely to make the verification work.

## Validation

Inspect/render all four guide templates for API 8080 + management 8081 and for
shared-port health. Check the verification URL against published ports or the
shown port-forward. Exercise representative commands against a local fixture
or available deployment; report precisely any deployment path not exercised.
Run shared link/sync checks and ensure no placeholders become unresolved.

## Open decisions

No owner decision is required to correct the verification guides. If a target
requires choosing a new public exposure policy, ask about that policy rather
than making it a prerequisite for every other guide.
