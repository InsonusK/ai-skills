SHELL := /bin/bash

.PHONY: init build run lint

# Run once after cloning: the module's dependencies, and the mutation tool the mutation
# kind would otherwise install on its first run.
init:
	go mod download
	go install github.com/go-gremlins/gremlins/cmd/gremlins@v0.6.0

build:
	go build -o bin/todo ./cmd/todo

# The base plateau runs its one startup command and exits.
run: build
	STARTUP_TITLES="buy milk,walk the dog" ./bin/todo

lint:
	go vet ./...

# Testing contract: test-kinds, test-kind-<kind>, test-report, test-readme-check, test-and-report.
# The kinds are the scripts in tools/testing/kinds/ - see solution-conformance-testing.
include tools/testing/testing.mk
