#!/usr/bin/env bash
# Source: skill api-develop-in-go
# The generated test kind: what is generated from the proto contracts is generated again
# into a scratch directory and compared with the committed files. No badge.
set -euo pipefail
source tools/testing/kind.sh

kind_mode "generated files compared with a fresh generation"

fresh=$(mktemp -d)
trap 'rm -rf "$fresh"' EXIT
# The same command as `make proto-gen`, written below $fresh instead of over the checkout:
# every contract folder of docs/integration/api, the module buf.yaml names, in one run.
# Slow on a CI runner (about two minutes) and fast locally: `go tool` compiles buf and the
# three protoc plugins from source when the build cache does not hold them, and the
# runner's restored cache does not - generating and comparing take seconds. If this ever
# matters, make the cache keep the compiled tools or install buf prebuilt in `make init`.
go tool buf generate --template buf.gen.yaml --output "$fresh"

status=0
for dir in gen/api docs/integration/api; do
  # Hand-written files beside the generated ones (the proto file, embed.go, a hand-written
  # openapi.json) exist only in the checkout.
  for file in $(cd "$fresh/$dir" && find . -type f | sort); do
    diff -u "$dir/$file" "$fresh/$dir/$file" || status=1
  done
done
# A committed generated file the contracts no longer produce.
for file in $(find gen/api -type f -name '*.pb.go'; find docs/integration/api -type f -name '*.swagger.json'); do
  [ -f "$fresh/$file" ] || { echo "$file is committed but no longer generated"; status=1; }
done

[ "$status" -eq 0 ] && echo "test-kind-$TEST_KIND: generated files are up to date" \
  || echo "test-kind-$TEST_KIND: generated files are stale - run 'make proto-gen' and commit the result" >&2
exit "$status"
