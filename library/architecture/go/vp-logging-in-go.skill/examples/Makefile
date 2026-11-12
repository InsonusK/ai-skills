SHELL := /bin/bash

.PHONY: init build run test lint

# Run once after cloning: the module's dependencies.
init:
	go mod download

build:
	go build -o bin/todo ./cmd/todo

# The base plateau runs its one startup command and exits.
run: build
	STARTUP_TITLES="buy milk,walk the dog" ./bin/todo

# Every Cucumber scenario of every package (godog runs inside go test).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-go; this example leaves it out.
test:
	go test ./...

lint:
	go vet ./...
