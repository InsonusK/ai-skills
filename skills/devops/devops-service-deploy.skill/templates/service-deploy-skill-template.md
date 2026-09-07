---
name: deploy-{service-name}
description: Deploy {service-name} with Docker Compose and Kubernetes.
whenToUse: when you need to deploy, update, or troubleshoot {service-name} in Docker Compose or Kubernetes.
tags:
  - stack
  - app-type/service
  - concern/ci
  - concern/documentation
  - docker
  - docker-compose
  - kubernetes
  - k8s
  - deployment
  - devops
---

# Goal
- Deploy {service-name} consistently on a single host with Docker Compose.
- Deploy {service-name} consistently in a Kubernetes cluster.
- Provide reusable configuration templates and concrete command examples.

# Core Principle
- The service is deployed as a container built from the repository's Dockerfile.
- Configuration is injected through environment variables and mounted files; secrets are never baked into the image.
- Docker Compose is used for single-host / development / staging deployments; Kubernetes is used for replicated production deployments.

# Rule

## MUST
- Build the image from the repository's Dockerfile before deploying.
  - Violation: deploying without building, or deploying an image built from stale source.
  - Risk: the running container does not match the current code, causing unpredictable failures or silent regressions.
  - Fix: run `docker build` or `docker compose build` from the repository root and verify the produced image tag before starting the deployment.
- Provide a `docker-compose.yml` with the service, health check, restart policy, and required dependencies.
  - Violation: the Docker Compose file is missing, lacks a health check, or omits a dependency.
  - Risk: the service starts without required dependencies, crashes silently, or cannot be restarted automatically after a failure.
  - Fix: include every dependency, a restart policy, and a health check endpoint in `docker-compose.yml`.
- Provide Kubernetes manifests: Deployment, Service, ConfigMap, Secret.
  - Violation: the Kubernetes manifests are incomplete or missing one of the required resources.
  - Risk: the workload cannot be scheduled, reached, or configured correctly in the cluster.
  - Fix: create Deployment, Service, ConfigMap, and Secret manifests tuned for this service; add Ingress or HPA when the service is exposed or scaled.
- Document every environment variable and its source.
  - Violation: environment variables, secrets, or volume mounts are undocumented or only described informally.
  - Risk: operators deploy the service with missing configuration and it fails at runtime, or secrets are guessed and mishandled.
  - Fix: list every variable, its purpose, default value if any, and whether it comes from a ConfigMap or Secret.

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

# Example
- Configuration templates: [`docker-compose.example.yml`](./docker-compose.example.yml), [`k8s-deployment.example.yml`](./k8s-deployment.example.yml), [`k8s-service.example.yml`](./k8s-service.example.yml), [`k8s-configmap.example.yml`](./k8s-configmap.example.yml), [`k8s-secret.example.yml`](./k8s-secret.example.yml), [`k8s-ingress.example.yml`](./k8s-ingress.example.yml).
- Deployment guides: [Docker Compose deploy guide](./docker-compose-deploy.example.md), [Kubernetes deploy guide](./k8s-deploy.example.md).

# Check list
- [ ] Image is built from the repository Dockerfile.
- [ ] Docker Compose file exists and starts the service.
- [ ] Kubernetes manifests exist and apply cleanly.
- [ ] All environment variables are documented.
- [ ] No real secret values are committed in examples.
