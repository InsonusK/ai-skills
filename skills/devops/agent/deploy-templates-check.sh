#!/bin/bash
# skills/devops/agent/deploy-templates-check.sh - ground truth for devops-service-deploy:
# the entrypoint does what the skill says, every template uses only the placeholders the skill
# lists, every filled manifest parses, and the filled chart lints and renders in both migration modes.
#   bash skills/devops/agent/deploy-templates-check.sh
set -uo pipefail
cd "$(git rev-parse --show-toplevel)" || exit 2
A=skills/devops/agent
S=skills/devops/core/devops-service-deploy.skill
fail=0
err() { echo "FAIL: $*"; fail=1; }
work=$(mktemp -d); trap 'rm -rf "$work"' EXIT

# 1. The entrypoint.
E=$S/assets/docker-entrypoint.sh
sh -n "$E" || err "docker-entrypoint.sh does not parse"
printf 'from-file' > "$work/secret"
out=$(env -i PATH="$PATH" TOKEN_FILE="$work/secret" sh "$E" sh -c 'printf "%s|%s" "$TOKEN" "${TOKEN_FILE:-unset}"' 2>&1)
[ "$out" = "from-file|unset" ] || err "NAME_FILE is not resolved to NAME: $out"
out=$(env -i PATH="$PATH" TOKEN=plain TOKEN_FILE="$work/secret" sh "$E" sh -c 'printf "%s" "$TOKEN"' 2>/dev/null)
[ "$out" = "plain" ] || err "a plain NAME must win over NAME_FILE: $out"
env -i PATH="$PATH" TOKEN_FILE="$work/missing" sh "$E" true 2>/dev/null && err "a missing secret file must stop the entrypoint"

# 2. Placeholders: only those the skill's table lists.
listed=$(grep -oE '^\| `\{[a-z-]+\}`' $S/devops-service-deploy.skill.md | grep -oE '\{[a-z-]+\}' | sort -u)
used=$(grep -rhoE '\{[a-z][a-z-]*\}' "$S/templates" | sort -u)
extra=$(comm -13 <(echo "$listed") <(echo "$used"))
[ -z "$extra" ] || err "placeholders in the templates that the skill does not list: $(echo $extra)"
grep -rnE '\{[A-Z_]+\}' "$S/templates" | grep -v '\${' && err "an upper-case {NAME} token in a template"

# 3. Fill the templates with sample values.
cp -R "$S/templates/." "$work/filled"
grep -rlE '\{[a-z][a-z-]*\}' "$work/filled" | while read -r f; do
  sed -i 's/{service-name}/orders/g; s#{registry}#ghcr.io/example#g; s/{version}/1.4.0/g; s/{namespace}/shop/g; s/{host-port}/8080/g; s/{date}/20261010/g' "$f"
done
T="$work/filled/deploy-{service-name}.skill.template/templates"

# 4. Every Compose, Stack, and Kubernetes manifest parses once filled.
if [ -d "$A/node_modules/yaml" ]; then
  for f in "$T"/docker/compose/*.yml "$T"/docker/stack/*.yml "$T"/k8s/*.yml; do
    node --input-type=module -e '
      import { readFileSync } from "node:fs";
      import { createRequire } from "node:module";
      const { parseAllDocuments } = createRequire(process.argv[2] + "/")("yaml");
      const docs = parseAllDocuments(readFileSync(process.argv[1], "utf8"));
      const errors = docs.flatMap((d) => d.errors);
      if (errors.length || docs.length === 0) { console.log(errors.map((e) => e.message).join("\n") || "empty"); process.exit(1); }
    ' "$f" "$PWD/$A/node_modules" || err "$(basename "$f") does not parse as YAML when filled"
  done
else
  echo "note: run 'npm install' in $A - the manifests are not parsed"
fi

# 5. The chart lints, and its one value switches the migration mode.
if command -v helm >/dev/null 2>&1; then
  C="$T/helm/chart-example"
  helm lint "$C" >"$work/lint.log" 2>&1 || { err "helm lint:"; cat "$work/lint.log"; }
  job=$(helm template orders "$C" --set migrate.mode=job 2>&1) || err "helm template, job mode: $job"
  start=$(helm template orders "$C" --set migrate.mode=onStart 2>&1) || err "helm template, onStart mode: $start"
  echo "$job" | grep -q '^kind: Job' || err "job mode renders no Job"
  echo "$start" | grep -q '^kind: Job' && err "onStart mode still renders the Job"
  echo "$job" | grep -A1 'name: MIGRATE_ON_START' | grep -q '"false"' || err "job mode does not set MIGRATE_ON_START to false"
  echo "$start" | grep -A1 'name: MIGRATE_ON_START' | grep -q '"true"' || err "onStart mode does not set MIGRATE_ON_START to true"
else
  echo "note: helm is not installed - the chart is not linted"
fi

[ "$fail" -eq 0 ] && echo "deploy-templates-check: all passed" || exit 1
