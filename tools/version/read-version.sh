#!/bin/sh
# tools/version/read-version.sh - prints the version recorded in a Go project: the root VERSION file.
# Source: skill devops-project-version-in-go. Copied verbatim; never edited in the project.
set -eu
tr -d '[:space:]' < VERSION
