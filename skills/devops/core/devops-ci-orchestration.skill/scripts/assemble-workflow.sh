#!/bin/sh
# assemble-workflow.sh - writes a project's workflow from a workflow template of the devops skills
# (devops-ci-orchestration). A template marks what only some project types have:
#
#   # docker:begin ... # docker:end    lines kept only for a project that ships a Docker image
#   some line # docker                 one such line
#
# and the same for `package` (a library published to a registry) and `app` (release binaries).
#
#   assemble-workflow.sh TEMPLATE [docker] [package=JOB-FILE] [app=JOB-FILE] > .github/workflows/NAME.yml
#
# A named type is kept and its markers are removed; a type not named is deleted. With =JOB-FILE the
# inside of the type's begin/end block is replaced by that file - the job a stack skill ships.
set -eu

[ $# -ge 1 ] || { echo "usage: assemble-workflow.sh TEMPLATE [docker] [package=JOB-FILE] [app=JOB-FILE]" >&2; exit 2; }
template=$1; shift
[ -f "$template" ] || { echo "assemble-workflow: no template $template" >&2; exit 2; }

keep=""; files=""
for arg in "$@"; do
  type=${arg%%=*}
  case "$type" in docker|package|app) ;; *) echo "assemble-workflow: unknown project type '$type'" >&2; exit 2 ;; esac
  keep="$keep $type"
  case "$arg" in
    *=*) file=${arg#*=}
         [ -f "$file" ] || { echo "assemble-workflow: no job file $file" >&2; exit 2; }
         files="$files $type=$file" ;;
  esac
done

awk -v keep="$keep" -v files="$files" '
BEGIN {
  n = split(keep, k, " ");  for (i = 1; i <= n; i++) kept[k[i]] = 1
  n = split(files, f, " "); for (i = 1; i <= n; i++) { split(f[i], p, "="); file[p[1]] = p[2] }
}
match($0, /^[ \t]*# (docker|package|app):begin[ \t]*$/) {
  type = $0; sub(/^[ \t]*# /, "", type); sub(/:begin.*/, "", type)
  block = type
  if ((type in kept) && (type in file)) { while ((getline line < file[type]) > 0) print line; close(file[type]) }
  next
}
match($0, /^[ \t]*# (docker|package|app):end[ \t]*$/) { block = ""; next }
block != "" { if ((block in kept) && !(block in file)) print; next }
match($0, /[ \t]+# (docker|package|app)$/) {
  type = substr($0, RSTART); sub(/^[ \t]+# /, "", type)
  if (type in kept) print substr($0, 1, RSTART - 1)
  next
}
{ print }
' "$template"
