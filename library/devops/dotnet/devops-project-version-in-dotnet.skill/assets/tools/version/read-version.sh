#!/bin/sh
# tools/version/read-version.sh - prints the version recorded in a .NET solution:
# the <Version> element of the root Directory.Build.props.
# Source: skill devops-project-version-in-dotnet. Copied verbatim; never edited in the project.
set -eu
v=$(sed -n 's:.*<Version>[[:space:]]*\([^<[:space:]]*\)[[:space:]]*</Version>.*:\1:p' Directory.Build.props | head -n 1)
[ -n "$v" ]
printf '%s\n' "$v"
