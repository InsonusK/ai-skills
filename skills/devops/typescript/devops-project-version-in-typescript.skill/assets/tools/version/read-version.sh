#!/bin/sh
# tools/version/read-version.sh - prints the version recorded in a TypeScript project:
# the version field of the root package.json.
# Source: skill devops-project-version-in-typescript. Copied verbatim; never edited in the project.
set -eu
node -p 'JSON.parse(require("fs").readFileSync("package.json", "utf8")).version'
