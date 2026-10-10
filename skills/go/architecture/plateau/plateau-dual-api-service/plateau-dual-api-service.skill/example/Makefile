SHELL := /bin/bash

.PHONY: build run lint proto-gen

VERSION := $(shell cat VERSION 2>/dev/null || echo dev)
LDFLAGS := -X github.com/example/linkcheck-service/internal/version.Version=$(VERSION)

build:
	go build -ldflags "$(LDFLAGS)" -o bin/linkcheck ./cmd/linkcheck

proto-gen:
	buf generate proto/linkcheck --template buf/buf.gen.yaml

run: build
	./bin/linkcheck

lint:
	go vet ./...

# Testing contract: test-kinds, test-kind-<kind>, test-report, test-readme-check, test-and-report.
# The kinds are the scripts in tools/testing/kinds/ - see solution-conformance-testing.
include tools/testing/testing.mk
