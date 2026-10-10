# Deploy {service-name} with Helm

Use this path instead of [`k8s-deploy.example.md`](../k8s/k8s-deploy.example.md) when the service
ships as a Helm chart — do not maintain both a plain-manifest deployment and a chart for the same
service. If the service has a migration step, this path needs no separate wait step of its own:
[`chart-example/templates/migrate-job.yaml`](./chart-example/templates/migrate-job.yaml) is wired as
a `helm.sh/hook: pre-install,pre-upgrade` resource, so `helm install`/`helm upgrade` blocks on it
natively — the plain-manifest path's `kubectl wait` step (step 3 there) has no equivalent here
because it isn't needed.

## Prerequisites
- `helm` (v3) and `kubectl`, both configured with access to the target cluster.
- Container image pushed to `{registry}/{service-name}:{version}`.
- Namespace `{namespace}` exists (`helm install --namespace {namespace} --create-namespace` also
  works for a first install).
- The Secret [`chart-example/templates/secret.yaml`](./chart-example/templates/secret.yaml)
  references (`{service-name}-secrets` by default) already exists in the target namespace, created
  out-of-band — never pass a real secret value through `--set` (it ends up in `helm get values` /
  release history in plaintext).

## Steps

### 1. Build and push the image
```bash
docker build -t {registry}/{service-name}:{version} .
docker push {registry}/{service-name}:{version}
```

### 2. Create the Secret (once per environment, before the first install)
```bash
kubectl create secret generic {service-name}-secrets \
  --namespace {namespace} \
  --from-literal=connection-string="$REAL_CONNECTION_STRING"
```

### 3. Install the chart
```bash
helm install {service-name} ./chart-example \
  --namespace {namespace} --create-namespace \
  --set image.tag={version} \
  --atomic
```
`--atomic` rolls the release back automatically if anything in it — including the migrate-job
hook — fails, instead of leaving a half-applied release behind.

If this service has no migration step, delete
[`chart-example/templates/migrate-job.yaml`](./chart-example/templates/migrate-job.yaml) and the
`migrate:` key from [`chart-example/values.yaml`](./chart-example/values.yaml) before installing —
don't just set `migrate.mode` to something else, the key shouldn't exist at all for a service this
doesn't apply to.

### 4. Verify the deployment
```bash
helm status {service-name} -n {namespace}
kubectl get pods -n {namespace} -l app.kubernetes.io/instance={service-name}
kubectl logs -n {namespace} -l app.kubernetes.io/instance={service-name},role!=migrate --tail=50
kubectl port-forward -n {namespace} svc/{service-name} {host-port}:80
curl http://localhost:{host-port}/health-check
```

## Update to a new version
```bash
helm upgrade {service-name} ./chart-example \
  --namespace {namespace} \
  --set image.tag={version} \
  --atomic
```
If the new version adds migrations, this is the **only** command needed for them too — the
`pre-upgrade` hook re-runs `migrate-job.yaml` before Helm touches the Deployment, and `--atomic`
rolls the whole upgrade back if the migration job fails. Nothing extra to run first, unlike the
plain-manifest or Docker Stack paths.

## Roll back
```bash
helm rollback {service-name} -n {namespace}
```
This rolls back the **application** release (the Deployment/Service/ConfigMap) to the previous
revision's values — it does not run `migrate-job.yaml`'s `Down` migrations or undo any schema
change the failed upgrade's hook already applied. See this catalog's own
`solution-go-db-migrations.skill/adr/migration-mode-per-platform.md` on why rollback policy is
"roll forward with a fix," not relying on `Down`, for exactly this reason.

## Uninstall
```bash
helm uninstall {service-name} -n {namespace}
```
The Secret created in step 2 is not part of the release (it was created out-of-band) and is not
removed by this — delete it separately if the environment itself is being decommissioned.
