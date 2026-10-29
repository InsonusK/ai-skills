SHELL := /bin/bash

.PHONY: init build run test lint

# Run once after cloning: the module's dependencies.
init:
	go mod download

build:
	go build -o bin/todo ./cmd/todo

# Runs the startup command once, planning its titles with the todo API's
# emulator (TODO_API_HOST=mock). Against a running todo service:
#   TODO_API_HOST=localhost TODO_API_HTTP_PORT=8080 make run
TODO_API_HOST ?= mock
run: build
	TODO_API_HOST=$(TODO_API_HOST) STARTUP_TITLES="buy milk,walk the dog" ./bin/todo

# Every Cucumber scenario of every package (godog runs inside go test).
# A real project adds its testing contract (kinds, reports, mutation) per
# solution-conformance-testing-in-go; this example leaves it out.
test:
	go test ./...

lint:
	go vet ./...
