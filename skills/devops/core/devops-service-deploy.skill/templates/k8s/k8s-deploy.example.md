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

### 3. Run the migration job and wait for completion (if `deploy/k8s/migrate-job.yml` applies)
A `Job`'s pod template is immutable, so a job with the same name from a previous deploy must be
deleted before re-applying it with a new image tag — `kubectl apply` alone fails on that field
change. Do not proceed to step 4 until `kubectl wait` reports the job complete:
```bash
kubectl delete job {service-name}-migrate -n {namespace} --ignore-not-found
kubectl apply -f deploy/k8s/migrate-job.yml
kubectl wait --for=condition=complete --timeout=120s job/{service-name}-migrate -n {namespace}
```
If the chart is Helm-based, wire the same Job as a `helm.sh/hook: pre-install,pre-upgrade` resource
instead — `helm upgrade` then blocks on it automatically and this step is unnecessary.

### 4. Apply workload and network resources
```bash
kubectl apply -f deploy/k8s/deployment.yml
kubectl apply -f deploy/k8s/service.yml
kubectl apply -f deploy/k8s/ingress.yml
```

### 5. Verify the deployment
```bash
kubectl get pods -n {namespace}
kubectl logs -n {namespace} -l app={service-name} --tail=50
kubectl port-forward -n {namespace} svc/{service-name} {host-port}:80
curl http://localhost:{host-port}/health-check
```

## Update to a new version
If the new version adds migrations, repeat step 3 with the new image tag first (deleting the old
job before re-applying, as shown there) and wait for it to complete — before touching the
Deployment. Then set the new image tag in `deploy/k8s/deployment.yml`, then:

```bash
kubectl apply -f deploy/k8s/deployment.yml
kubectl rollout status deployment/{service-name} -n {namespace}
```

## Roll back
```bash
kubectl rollout undo deployment/{service-name} -n {namespace}
```
