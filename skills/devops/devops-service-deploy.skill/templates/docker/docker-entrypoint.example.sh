#!/bin/sh
# Resolves the {NAME}_FILE secret-file convention before exec'ing the real
# process, so every environment variable the service reads can be supplied
# either directly (NAME=value) or as a file path (NAME_FILE=/path/to/file) —
# the same convention official images (postgres, mysql, ...) use for
# Docker/Kubernetes/Swarm secrets mounted as files.
set -eu

for file_var_name in $(env | grep -E '^[A-Za-z_][A-Za-z0-9_]*_FILE=' | cut -d= -f1); do
  var_name="${file_var_name%_FILE}"
  file_path=$(eval "printf '%s' \"\${$file_var_name}\"")

  if eval "[ -n \"\${$var_name:-}\" ]"; then
    echo "docker-entrypoint: both $var_name and $file_var_name are set; ignoring $file_var_name" >&2
    continue
  fi
  if [ ! -f "$file_path" ]; then
    echo "docker-entrypoint: $file_var_name points at missing file '$file_path'" >&2
    exit 1
  fi

  var_value=$(cat "$file_path")
  export "$var_name=$var_value"
  unset "$file_var_name"
done

exec "$@"
