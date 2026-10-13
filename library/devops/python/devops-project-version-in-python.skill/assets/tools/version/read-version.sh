#!/bin/sh
# tools/version/read-version.sh - prints the version recorded in a Python project:
# project.version of the root pyproject.toml.
# Source: skill devops-project-version-in-python. Copied verbatim; never edited in the project.
set -eu
python3 -c 'import tomllib; print(tomllib.load(open("pyproject.toml", "rb"))["project"]["version"])'
