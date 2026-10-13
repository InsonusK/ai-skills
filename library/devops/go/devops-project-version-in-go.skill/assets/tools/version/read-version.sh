#!/bin/sh
# tools/version/read-version.sh - prints the version recorded in a Go project:
# the string of `var Version = "..."` in internal/version/version.go.
# Source: skill devops-project-version-in-go. Copied verbatim; never edited in the project.
set -eu
v=$(sed -n 's/^var[[:space:]]\{1,\}Version[[:space:]]*=[[:space:]]*"\([^"]*\)".*/\1/p' internal/version/version.go | head -n 1)
[ -n "$v" ]
printf '%s\n' "$v"
