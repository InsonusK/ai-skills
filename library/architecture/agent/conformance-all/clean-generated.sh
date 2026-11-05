#!/bin/bash
# Deletes the code Reqnroll generated from a .feature that no longer exists.
cd "$(dirname "$0")/../../dotnet" && find . -name '*.feature.cs' -not -path '*/obj/*' | while read -r f; do [ -f "${f%.cs}" ] || rm -f "$f"; done
