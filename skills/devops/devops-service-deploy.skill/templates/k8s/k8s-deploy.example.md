# Deploy {service-name} to Kubernetes

## Prerequisites
- `kubectl` configured with access to the target cluster
- Container image pushed to `{registry}/{service-name}:{version}`
- Namespace `{namespace}` exists or will be created

## Steps

### 1. Create namespace
```bash
kubectl apply -f deploy/k8s/namespace.yml
```

### 2. Apply configuration and secrets
Edit `deploy/k8s/secret.yml` to inject real secret values via a secret-management tool; never commit plain secrets.

```bash
kubectl apply -f deploy/k8s/configmap.yml
kubectl apply -f deploy/k8s/secret.yml
```

### 3. Apply workload and network resources
```bash
kubectl apply -f deploy/k8s/deployment.yml
kubectl apply -f deploy/k8s/service.yml
kubectl apply -f deploy/k8s/ingress.yml
```

### 4. Verify the deployment
```bash
kubectl get pods -n {namespace}
kubectl logs -n {namespace} -l app={service-name} --tail=50
kubectl port-forward -n {namespace} svc/{service-name} {host-port}:80
curl http://localhost:{host-port}/health
```

## Update to a new version
Set the new image tag in `deploy/k8s/deployment.yml`, then:

```bash
kubectl apply -f deploy/k8s/deployment.yml
kubectl rollout status deployment/{service-name} -n {namespace}
```

## Roll back
```bash
kubectl rollout undo deployment/{service-name} -n {namespace}
```
