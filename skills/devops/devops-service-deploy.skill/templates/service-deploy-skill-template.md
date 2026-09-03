---
name: deploy-{service-name}
description: Deploy {service-name} with Docker Compose and Kubernetes.
whenToUse: when you need to deploy, update, or troubleshoot {service-name} in Docker Compose or Kubernetes.
tags:
  - devops
  - deployment
  - docker
  - docker-compose
  - kubernetes
  - k8s
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
- Provide a `docker-compose.yml` with the service, health check, restart policy, and required dependencies.
- Provide Kubernetes manifests: Deployment, Service, ConfigMap, Secret.
- Document every environment variable and its source.

## SHOULD
- Include an Ingress for external HTTP access in Kubernetes.
- Include resource requests and limits.
- Provide separate override files for different environments.

## MAY
- Provide a Helm chart.
- Provide a Kustomize overlay.

# Configuration

## Docker Compose
See `docker-compose.yml` (or `deploy/docker-compose.yml`) in the service repository.

## Kubernetes
See the `deploy/k8s/` directory (or equivalent) in the service repository.

# Deployment example

## Docker Compose

```bash
# 1. Build the image
docker compose build

# 2. Start the service
docker compose up -d

# 3. Verify
docker compose ps
docker compose logs --tail=50 {service-name}
curl http://localhost:{host-port}/health
```

## Kubernetes

```bash
# 1. Create namespace
kubectl apply -f deploy/k8s/namespace.yml

# 2. Apply ConfigMap and Secret
kubectl apply -f deploy/k8s/configmap.yml
kubectl apply -f deploy/k8s/secret.yml

# 3. Apply Deployment and Service
kubectl apply -f deploy/k8s/deployment.yml
kubectl apply -f deploy/k8s/service.yml

# 4. Verify
kubectl get pods -n {namespace}
kubectl logs -n {namespace} -l app={service-name} --tail=50
kubectl port-forward -n {namespace} svc/{service-name} {host-port}:80
curl http://localhost:{host-port}/health
```

# Check list
- [ ] Image is built from the repository Dockerfile.
- [ ] Docker Compose file exists and starts the service.
- [ ] Kubernetes manifests exist and apply cleanly.
- [ ] All environment variables are documented.
- [ ] No secrets are committed in plain text.
