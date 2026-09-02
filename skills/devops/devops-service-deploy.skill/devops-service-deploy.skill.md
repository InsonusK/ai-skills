---
name: devops-service-deploy
description: Require every service repository to contain a deploy skill that describes how to deploy the service with Docker Compose and Kubernetes, including configuration templates and deployment examples.
whenToUse: when you are creating or updating a service and need to produce deployment documentation and artifacts for Docker Compose and Kubernetes.
tags:
  - devops
  - deployment
  - docker
  - docker-compose
  - kubernetes
  - k8s
  - concern/operations
  - stack
---

# Goal
- Every service repository must contain its own deployment skill so that any agent or operator can deploy the service consistently.
- The deployment skill must cover both Docker Compose (local / single-node) and Kubernetes (cluster) deployment paths.
- It must provide ready-to-use configuration templates and concrete deployment examples.

# Core Principle
- Deployment knowledge belongs in the same repository as the service it describes, not in a shared wiki or in an agent's memory.
- The deployment skill is created and updated together with the service code — never left as an afterthought.
- The skill is stack-agnostic in structure but concrete in content: it names real images, ports, volumes, environment variables, and dependencies for this service.
- Docker Compose is the default path for development, staging, and single-host production; Kubernetes is the path for multi-host / replicated production.

# Rule

## MUST
- When creating or modifying a service, create or update the deployment skill at `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md` in the service repository.
  - Risk: without a local deploy skill, every future deployment depends on implicit knowledge and ad-hoc commands.
  - Fix: keep the skill next to the service code and update it whenever deployment-relevant facts change.
- The deployment skill must contain a `docker-compose.yml` template or a reference to a checked-in `docker-compose.yml` / `docker-compose.prod.yml`.
  - Risk: operators cannot spin up the service locally or on a single host.
  - Fix: provide a complete Docker Compose file with image, ports, environment variables, health checks, volumes, and dependencies.
- The deployment skill must contain Kubernetes manifests or a Helm chart for the service.
  - Risk: cluster deployments are inconsistent or require manual recreation of resources.
  - Fix: provide Deployment, Service, ConfigMap, and Secret manifests (and Ingress / HPA when applicable).
- The deployment skill must document the exact environment variables, secrets, and volume mounts the service needs.
  - Risk: the service starts with missing configuration and fails at runtime.
  - Fix: list every variable, its purpose, default value (if any), and whether it comes from a Secret or ConfigMap.
- The deployment skill must include step-by-step deployment examples for both Docker Compose and Kubernetes.
  - Risk: operators guess the correct commands and miss flags such as `--build`, `--env-file`, or namespace/context.
  - Fix: provide copy-paste-ready command sequences, including prerequisites and verification steps.
- Keep example values consistent with the service's actual defaults (port, image name, application name, config keys).
  - Risk: examples drift from reality and cause failed first-time deployments.
  - Fix: derive example values from the service's Dockerfile / source configuration and review them when those change.

## SHOULD
- Include health checks and restart policies in the Docker Compose template.
- Include resource requests and limits in Kubernetes Deployment manifests.
- Include a namespace and label conventions in Kubernetes manifests.
- Provide separate compose files for development and production (`docker-compose.yml` and `docker-compose.override.yml` / `docker-compose.prod.yml`).
- Provide a `Makefile` or `deploy.sh` helper script that wraps the most common deploy commands.
- Pin image tags explicitly instead of using `latest`.

## MAY
- Provide a Helm chart in addition to plain manifests.
- Provide a Kustomize overlay.
- Provide a `skaffold.yaml` for local Kubernetes development.

## MUST NOT
- Store real secret values inside the skill or example files.
- Use `latest` as the only image tag in production-oriented examples without explaining the risk.
- Write deployment instructions that only work in one specific environment (e.g. your laptop) without documenting assumptions.

# Anti-patterns
- **Put deployment docs in a separate wiki only**
  - Consequence: the docs rot and do not follow the service through version control.
  - Instead: keep deployment knowledge as a skill inside the service repository.
- **Provide only Kubernetes manifests and ignore Docker Compose**
  - Consequence: developers and small installs cannot easily run the service.
  - Instead: always cover both Docker Compose and Kubernetes.
- **Hard-code environment-specific values in manifests**
  - Consequence: every environment requires editing the skill or manifests.
  - Instead: use placeholders, ConfigMaps, and environment-specific overlay files.
- **Skip verification steps**
  - Consequence: a silently broken deployment is mistaken for a successful one.
  - Instead: include commands that check container health / pod readiness / endpoint responses.

# Example
See the templates in [./templates](./templates):
- [`service-deploy-skill-template.md`](./templates/service-deploy-skill-template.md) — skeleton for the deployment skill the agent must create in the service repository.
- [`docker-compose.example.yml`](./templates/docker-compose.example.yml) — Docker Compose template for a generic service.
- [`docker-compose-deploy.example.md`](./templates/docker-compose-deploy.example.md) — step-by-step Docker Compose deploy guide.
- [`k8s-deployment.example.yml`](./templates/k8s-deployment.example.yml) — Kubernetes Deployment template.
- [`k8s-service.example.yml`](./templates/k8s-service.example.yml) — Kubernetes Service template.
- [`k8s-configmap.example.yml`](./templates/k8s-configmap.example.yml) — Kubernetes ConfigMap template.
- [`k8s-secret.example.yml`](./templates/k8s-secret.example.yml) — Kubernetes Secret template.
- [`k8s-ingress.example.yml`](./templates/k8s-ingress.example.yml) — Kubernetes Ingress template.
- [`k8s-deploy.example.md`](./templates/k8s-deploy.example.md) — step-by-step Kubernetes deploy guide.

# Check list
- [ ] A deployment skill exists at `skills/devops/deploy-{service-name}.skill/deploy-{service-name}.skill.md`.
- [ ] The skill contains a Docker Compose configuration template and a deployment example.
- [ ] The skill contains Kubernetes manifests (Deployment, Service, ConfigMap, Secret) and a deployment example.
- [ ] Environment variables, secrets, and volume mounts are documented.
- [ ] No real secret values are committed in examples.
- [ ] Image tags are pinned or the use of `latest` is explicitly justified.
- [ ] Verification commands are included in both Docker Compose and Kubernetes examples.
